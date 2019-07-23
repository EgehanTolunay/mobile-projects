using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EditorUtil
{
    [MenuItem("Utils/Create Empty Object On Center")]
    public static void CreateEmptyObjectOnCenter()
    {
        Vector3 position = Vector3.zero;

        Vector3 total = Vector3.zero;
        foreach(GameObject go in Selection.gameObjects)
        {
            total += go.transform.position;
        }

        position = total / Selection.gameObjects.Length;

        GameObject newObject = new GameObject("Center Object");

        newObject.transform.position = position;
        newObject.transform.rotation = Quaternion.identity;
    }
}
#endif

public static class ExtensionUtil
{
    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public static void AddOrCreate<TKey,TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key].Add(value);
        }
        else
        {
            dict.Add(key, new List<TValue>() { value });
        }
    }

    public static void AddOrCreate<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key].Add(value);
        }
        else
        {
            dict.Add(key, new HashSet<TValue>() { value });
        }
    }

    public static void DecreaseOrRemove<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key].Remove(value);

            if(dict[key].Count == 0)
            {
                dict.Remove(key);
            }
        }
    }
}

#region POOLING

public interface IPoolInitialized
{
    void InitializeAsPoolItem();
}

public interface IPoolInitialized<T>
{
    void InitializeAsPoolItem(T arg);
}

public interface IPoolInstallProcesser
{
    void ApplyInstallProcess();
}

public interface IPoolInstallProcesser<TArg>
{
    void ApplyInstallProcess(TArg arg);
}

public interface IPoolReturnProcesser
{
    void ApplyReturnProcess();
}

public interface IPoolReturnProcesser<TArg>
{
    void ApplyReturnProcess(TArg arg);
}

public class CataloguePool<TKey, TItem> where TItem : Component
{
    private Dictionary<TKey, TItem> catalogue;
    private Dictionary<TKey, List<TItem>> availableItems;
    private HashSet<TItem> instantiatedItemSet;
    private List<TItem> instantiatedItemList;

    public int TypeCount
    {
        get { return catalogue.Count; }
    }

    public int InstantiatedCount
    {
        get { return instantiatedItemSet.Count; }
    }

    public TItem LastInstantiated
    {
        get { return instantiatedItemList[InstantiatedCount - 1]; }
    }

    public List<TItem> InstantiatedListClone
    {
        get
        {
            List<TItem> newItemList = new List<TItem>(instantiatedItemList);
            return newItemList;
        }
    }

    public List<TItem> InstantiatedList
    {
        get
        {
            return instantiatedItemList;
        }
    }

    public TItem this[int index]
    {
        get
        {
            return instantiatedItemList[index];
        }
    }

    [SerializeField]
    private int initialCountFromEach = 10;

    [SerializeField]
    private Transform poolContainerTransform;

    public CataloguePool(int initialCountFromEach, Transform poolContainerTransform, Dictionary<TKey, TItem> catalogue)
    {
        this.initialCountFromEach = initialCountFromEach;
        this.poolContainerTransform = poolContainerTransform;

        this.catalogue = new Dictionary<TKey, TItem>(catalogue);
        availableItems = new Dictionary<TKey, List<TItem>>();
        instantiatedItemSet = new HashSet<TItem>();
        instantiatedItemList = new List<TItem>();
    }

    public TItem[] InitializePool()
    {
        List<TItem> createdItems = new List<TItem>();
        foreach (TKey key in catalogue.Keys)
        {
            for (int i = 0; i < initialCountFromEach; i++)
            {
                createdItems.Add(CreatePoolTile(key));
            }
        }

        return createdItems.ToArray();
    }

    TItem CreatePoolTile(TKey key)
    {
        GameObject newClone = GameObject.Instantiate(catalogue[key].gameObject, poolContainerTransform);

        TItem newItem = newClone.GetComponent<TItem>();

        if(newItem == null)
        {
            newItem = newClone.GetComponent<TItem>();
        }

        if(newItem != null)
        if (availableItems.ContainsKey(key))
        {
            availableItems[key].Add(newItem);
        }
        else
        {
            availableItems.Add(key, new List<TItem>() { newItem });
        }

        return newItem;
    }

    public TItem GetFromPool(TKey key)
    {
        if (!catalogue.ContainsKey(key)) return null;

        TItem item;

        if (availableItems[key].Count == 0) item = CreatePoolTile(key);

        item = availableItems[key][0];
        availableItems[key].RemoveAt(0);
        instantiatedItemSet.Add(item);
        instantiatedItemList.Add(item);

        return item;
    }

    public TItem GetFromPool(TKey key, UnityAction<TItem> initialization)
    {
        if (!catalogue.ContainsKey(key)) return null;

        TItem item;

        if (availableItems[key].Count == 0) { item = CreatePoolTile(key); initialization(item); }

        item = availableItems[key][0];
        availableItems[key].RemoveAt(0);
        instantiatedItemSet.Add(item);
        instantiatedItemList.Add(item);

        return item;
    }

    public void ReturnToPool(TKey key, TItem item)
    {
        if (!instantiatedItemSet.Contains(item)) return;

        instantiatedItemSet.Remove(item);
        instantiatedItemList.Remove(item);
        availableItems[key].Add(item);
    }
}

public class SingularPool<TItem> where TItem : Component
{
    private TItem clonedObject;

    private List<TItem> availableItemList;
    private HashSet<TItem> instantiatedItemSet;
    private List<TItem> instantiatedItemList;

    [SerializeField]
    private int initialCountFromEach = 10;

    [SerializeField]
    private Transform poolContainerTransform;

    public int InstantiatedCount
    {
        get { return instantiatedItemSet.Count; }
    }

    public TItem LastInstantiated
    {
        get { if (instantiatedItemList.Count != 0) return instantiatedItemList[InstantiatedCount - 1];
            else return null;
        }
    }

    public List<TItem> InstantiatedListClone
    {
        get
        {
            List<TItem> newItemList = new List<TItem>(instantiatedItemList);
            return newItemList;
        }
    }

    public List<TItem> InstantiatedList
    {
        get
        {
            return instantiatedItemList;
        }
    }

    public SingularPool(int initialCountFromEach, Transform poolContainerTransform, TItem clonedObject)
    {
        this.clonedObject = clonedObject;

        this.initialCountFromEach = initialCountFromEach;
        this.poolContainerTransform = poolContainerTransform;

        availableItemList = new List<TItem>();
        instantiatedItemSet = new HashSet<TItem>();
        instantiatedItemList = new List<TItem>();
    }

    public TItem[] InitializePool()
    {
        List<TItem> createdItems = new List<TItem>();

        for (int i = 0; i < initialCountFromEach; i++)
        {
            createdItems.Add(CreatePoolItem());
        }

        return createdItems.ToArray();
    }

    TItem CreatePoolItem()
    {
        GameObject newClone = GameObject.Instantiate(clonedObject.gameObject, poolContainerTransform);

        TItem newItem = newClone.GetComponent<TItem>();

        if (newItem == null)
        {
            newItem = newClone.GetComponent<TItem>();
        }

        if (newItem != null)
            availableItemList.Add(newItem);

        return newItem;
    }

    public TItem GetFromPool()
    {
        TItem item;

        if (availableItemList.Count == 0) item = CreatePoolItem();

        item = availableItemList[0];
        availableItemList.RemoveAt(0);
        instantiatedItemSet.Add(item);
        instantiatedItemList.Add(item);

        return item;
    }

    public TItem GetFromPool(UnityAction<TItem> initialization)
    {
        TItem item;

        if (availableItemList.Count == 0) { item = CreatePoolItem(); initialization(item); }

        item = availableItemList[0];
        availableItemList.RemoveAt(0);
        instantiatedItemSet.Add(item);
        instantiatedItemList.Add(item);

        return item;
    }

    public void ReturnToPool(TItem item)
    {
        if (!instantiatedItemSet.Contains(item)) return;

        instantiatedItemSet.Remove(item);
        instantiatedItemList.Remove(item);
        availableItemList.Add(item);
    }

    #endregion

}

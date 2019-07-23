using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Khalreon.Attributes;

public class ObjectPool : SecureInitializeBehaviour
{
    private static Dictionary<string, ObjectPool> _poolDictionary;

    [Header("Object Pool")]

    [SerializeField]
    private string poolName = "Pool";

    [SerializeField]
    private GameObject clonedPrefab;

    [Header("Filled Lists (Don't Modify)")]
    [SerializeField]
    private List<PoolElement> poolElementList = new List<PoolElement>();

    [SerializeField]
    private List<PoolElement> availableElements = new List<PoolElement>();

    private HashSet<PoolElement> poolElementSet = new HashSet<PoolElement>();

    [Header("Options")]

    [SerializeField]
    private int minimumSize;

    [SerializeField]
    private int initialSize;

    [SerializeField]
    private bool isAdaptive;

    [Space]

    [SerializeField]
    private Timer frequencyUpdateTimer = new Timer(5);

    [SerializeField]
    private Timer resizeTimer = new Timer(30);

    [Header("Output")]

    [SerializeField][NumericLabel]
    private int totalSpawnedCount = 0;

    private List<int> usageList = new List<int>();

    [SerializeField][NumericLabel]
    private int averageUsageFrequency;

    [SerializeField][NumericLabel]
    private int perUpdateUsageCount;

    public static ObjectPool GetObjectPool(string name)
    {
        if (_poolDictionary.ContainsKey(name))
        {
            return _poolDictionary[name];
        }
        else
        {
            Debug.LogError("An object pool with the given name doesn't exists!");
            return null;
        }
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        if(_poolDictionary == null)
        {
            _poolDictionary = new Dictionary<string, ObjectPool>();
        }

        int duplicateCount = 0;
        while(_poolDictionary.ContainsKey(poolName))
        {
            duplicateCount += 1;
            poolName = poolName + duplicateCount.ToString(); 
        }

        _poolDictionary.Add(poolName, this);

        if (isAdaptive)
        {
            enabled = true;
        }
        else enabled = false;

        ExtendPool(minimumSize);

        if(initialSize < minimumSize)
        {
            initialSize = minimumSize;
        }

        int extra = initialSize - minimumSize;

        ExtendPool(extra);
    }

    private void Update()
    {
        frequencyUpdateTimer.Tick(1);
        resizeTimer.Tick(1);

        if (frequencyUpdateTimer.IsFinished)
        {
            usageList.Add(Mathf.RoundToInt((perUpdateUsageCount)));

            perUpdateUsageCount = 0;

            int total = 0;
            foreach(int i in usageList)
            {
                total += i;
            }

            averageUsageFrequency = total / usageList.Count;

            frequencyUpdateTimer.Reset();
        }

        if (resizeTimer.IsFinished)
        {
            int unnecessaryCount = availableElements.Count - averageUsageFrequency - minimumSize;

            if(unnecessaryCount <= 0) { Debug.Log("No resize necessary."); }
            else
            {
                Debug.Log("Removing " + unnecessaryCount + " unnecessary elements from pool");
            }

            for (int i = 0; i<unnecessaryCount; i++)
            {
                DestroyElement(availableElements[0]);
            }

            usageList.Clear();

            resizeTimer.Reset();
        }
    }

    private void SpawnElement()
    {
        GameObject newClone = Instantiate(clonedPrefab, transform);

        PoolElement element = newClone.GetComponent<PoolElement>();
        
        if(element == null)
        {
            element = newClone.AddComponent<PoolElement>();
        }

        totalSpawnedCount += 1;

        element.Initialize(totalSpawnedCount, this);

        poolElementList.Add(element);

        poolElementSet.Add(element);

        availableElements.Add(element);
    }

    private void DestroyElement(PoolElement element)
    {
        if (element.IsActive)
        {
            Debug.Log("Something went wrong because an active pool element should not be destroyed from the pool. Think again about destroying pool element as well.");
            return;
        }

        if (poolElementSet.Contains(element))
        {
            poolElementList.Remove(element);

            poolElementSet.Remove(element);

            availableElements.Remove(element);

            Destroy(element.gameObject);
        }
    }

    public void ExtendPool(int count)
    {
        for(int i = 0; i<count; i++)
        {
            SpawnElement();
        }
    }

    public GameObject InstantiateElement(Vector3 position, Quaternion rotation)
    {
        perUpdateUsageCount += 1;

        if(availableElements.Count == 0)
        {
            ExtendPool(1);
        }

        PoolElement selectedElement = availableElements[0];

        availableElements.Remove(selectedElement);

        selectedElement.OnSelectedForInstantiate(position, rotation);

        //Select an appropriate pooled object from the list.

        //When get activated fire off OnInstantiate event on element's PoolElement script.

        return selectedElement.gameObject;
    }

    public void NotifyReturn(PoolElement returningElement)
    {
        availableElements.Add(returningElement);
    }
}

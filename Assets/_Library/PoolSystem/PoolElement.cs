using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Khalreon.Attributes;

public class PoolElement : MonoBehaviour
{
    [SerializeField][Disable]
    private ObjectPool assignedPool;

    [SerializeField]
    [BoolLabel]
    private bool isActive;
    public bool IsActive
    {
        get { return isActive; }
    }

    [SerializeField][NumericLabel]
    private int poolElementId;

    public UnityEvent OnSpawn;

    public UnityEvent OnInstantiate;

    public UnityEvent OnReturn;

    public void Initialize(int id, ObjectPool pool)
    {
        isActive = false;

        poolElementId = id;

        assignedPool = pool;

        OnSpawn.Invoke();

        gameObject.SetActive(false);
    }

    public void ReturnPool()
    {
        if(assignedPool == null) { Destroy(gameObject); return; }

        OnReturn.Invoke();

        isActive = false;

        assignedPool.NotifyReturn(this);

        gameObject.SetActive(false);
    }

    public void OnSelectedForInstantiate(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        gameObject.SetActive(true);
        
        OnInstantiate.Invoke();
        
        isActive = true;
    }
}

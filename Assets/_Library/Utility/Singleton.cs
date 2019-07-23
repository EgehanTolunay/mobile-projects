using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("[Singleton] Singleton of type " + typeof(T).ToString() + " doesn't exist in the scene.");
            }

            return instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;

            Debug.Log("[Singleton] Singleton with type " + typeof(T).ToString() + " has been destroyed.");
        }
        else
        {
            Debug.Log("[Singleton] Clone with type " + typeof(T).ToString() + " has been destroyed.");
        }
    }

    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = GetComponent<T>();
        }
        else
        {
            Debug.LogWarning("[Singleton] There is more than one Singleton objects.");
        }
    }
}

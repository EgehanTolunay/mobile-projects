using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ScriptOrder(1)]
public class MonoUpdater : Singleton<MonoUpdater>
{
    private System.Action onUpdate;

    private Dictionary<Component,System.Action> subscribers = new Dictionary<Component, System.Action>();

    public static void Subscribe(Component obj,System.Action action)
    {
        if(Instance != null)
        {
            if (Instance.subscribers.ContainsKey(obj)) return;

            Instance.subscribers.Add(obj, action);

            Instance.onUpdate += action;
        }
    }

    public static void Unsubscribe(Component obj)
    {
        if (Instance != null)
        {
            if (!Instance.subscribers.ContainsKey(obj)) return;

            Instance.onUpdate -= Instance.subscribers[obj];
        }
    }

    private void Update()
    {
        foreach(Component comp in subscribers.Keys)
        {
            if (comp.gameObject.activeInHierarchy)
            {
                subscribers[comp].Invoke();
            }
        }
    }
}

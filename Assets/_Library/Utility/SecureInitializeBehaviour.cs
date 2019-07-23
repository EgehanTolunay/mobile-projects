using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Khalreon.Attributes;
//Provides safe to disable and enable framework.

public abstract class SecureInitializeBehaviour : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Initialization will happen on Awake")]
    private bool autoInitialize = true;

    //[SerializeField]
    //[BoolLabel]
    private bool isInitialized = false;
    public bool IsInitialized
    {
        get { return isInitialized; }
    }

    //[SerializeField]
    //[BoolLabel]
    private bool isStarted = false;
    public bool IsStarted
    {
        get
        {
            return isStarted;
        }
    }

    private void Awake()
    {
        if (!autoInitialize) return;

        if (!isInitialized)
        {
            OnInitialize();
            isInitialized = true;
        }
    }

    private void OnEnable()
    {
        if (!autoInitialize) return;

        if (!isInitialized)
        {
            OnInitialize();
            isInitialized = true;

            OnStart();
            isStarted = true;
        }

    }

    /// <summary>
    /// Retrieval of required references.
    /// </summary>
    public void Initialize()
    {
        OnInitialize();
        isInitialized = true;
    }

    protected virtual void OnInitialize()
    {

    }

    public void Start()
    {
        if (!autoInitialize) return;

        OnStart();
        isStarted = true;
    }

    /// <summary>
    /// How does this behaviour begin it's job what is it's state at begininning. what does it do. This phase should not happen unless an object is initialized. Happens first choise in Start(). if disabled, starts on OnEnable();
    /// </summary>
    protected virtual void OnStart()
    {

    }

}




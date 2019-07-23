using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Khalreon.Attributes;

[System.Serializable]
public class TriggerData
{
    public GameObject otherGameObject;
    public Transform otherTransform;
    public TagHolder otherTagHolder;
    public TriggerDetectee triggerDetectee;
    public TriggerDetector triggerDetector;
}

[System.Serializable]
public class TriggerDataEvent : UnityEvent<TriggerData>
{

}

[RequireComponent(typeof(TagHolder))]
public abstract class TriggerDetectionBase : MonoBehaviour
{
    [SerializeField]
    [Disable]
    protected TagHolder tagHolder;
    public TagHolder TagHolder
    {
        get { return tagHolder; }
    }

    [SerializeField]
    [Disable]
    [Tooltip("This component requires either a collider with isTrigger checked, or a Rigidbody that has trigger colliders in the hierarchy.")]
    protected new Rigidbody rigidbody;
    public Rigidbody Rigidbody
    {
        get { return rigidbody; }
    }

    [SerializeField][Disable][Tooltip("This component requires either a collider with isTrigger checked, or a Rigidbody that has trigger colliders in the hierarchy.")]
    protected new Collider collider;
    public Collider Collider
    {
        get { return collider; }
    }

    protected TriggerDataEvent triggerEnter = new TriggerDataEvent();
    public TriggerDataEvent TriggerEnter
    {
        get { return triggerEnter; }
    }

    protected TriggerDataEvent triggerExit = new TriggerDataEvent();
    public TriggerDataEvent TriggerExit
    {
        get { return triggerExit; }
    }

    protected virtual void OnValidate()
    {
        if (Application.isPlaying) return;

        tagHolder = GetComponent<TagHolder>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }
}

public class TriggerDetector : TriggerDetectionBase
{
    [SerializeField]
    private bool prioritiseRigidbody;

    [SerializeField][Tooltip("Ignores colliders without rigidbody.")]
    private bool rigidbodyCompositeMode;

    private Dictionary<Rigidbody,List<Collider>> rigidbodyDictionary = new Dictionary<Rigidbody, List<Collider>>();

    private void OnTriggerEnter(Collider other)
    {
        InvokeTriggerEvent(other, triggerEnter, true);
    }

    private void OnTriggerExit(Collider other)
    {
        InvokeTriggerEvent(other, triggerExit, false);
    }

    void FillData(TriggerData data, TriggerDetectee detectee)
    {
        data.triggerDetectee = detectee;
        data.triggerDetector = this;
        data.otherTagHolder = detectee.TagHolder;
        data.otherTransform = detectee.transform;
        data.otherGameObject = detectee.gameObject;
    }

    void InvokeTriggerEvent(Collider other,TriggerDataEvent e, bool isEnter)
    {
        Rigidbody otherRigidbody = other.attachedRigidbody;

        TriggerDetectee detectee = null;

        TriggerData data = new TriggerData();

        bool hasEntered = false;
        bool hasExited = false;

        if (!rigidbodyCompositeMode)
        {
            if (!prioritiseRigidbody)
            {
                detectee = other.GetComponent<TriggerDetectee>();

                if (detectee != null)
                {
                    FillData(data, detectee);

                    if (isEnter) detectee.NotifyTriggerEnter(this, gameObject);
                    else detectee.NotifyTriggerExit(this, gameObject);

                    e.Invoke(data);

                    return;
                }

                if (otherRigidbody != null)
                {
                    detectee = otherRigidbody.GetComponent<TriggerDetectee>();

                    if (detectee != null)
                    {
                        FillData(data, detectee);

                        if (isEnter) detectee.NotifyTriggerEnter(this, gameObject);
                        else detectee.NotifyTriggerExit(this, gameObject);

                        e.Invoke(data);
                    }
                }
            }
            else
            {
                if (otherRigidbody != null)
                {
                    detectee = otherRigidbody.GetComponent<TriggerDetectee>();

                    if (detectee != null)
                    {
                        if (isEnter) detectee.NotifyTriggerEnter(this, gameObject);
                        else detectee.NotifyTriggerExit(this, gameObject);
                        FillData(data, detectee);
                        e.Invoke(data);

                        return;
                    }


                }

                detectee = other.GetComponent<TriggerDetectee>();

                if (detectee != null)
                {
                    FillData(data, detectee);

                    if (isEnter) detectee.NotifyTriggerEnter(this, gameObject);
                    else detectee.NotifyTriggerExit(this, gameObject);

                    e.Invoke(data);
                }
            }
        }
        else
        {
            if (otherRigidbody != null)
            {
                if (isEnter)
                {
                    if (!rigidbodyDictionary.ContainsKey(otherRigidbody))
                    {
                        rigidbodyDictionary.Add(otherRigidbody, new List<Collider>() { other });
                        hasEntered = true;
                    }
                    else
                    {
                        rigidbodyDictionary[otherRigidbody].Add(other);
                    }
                }
                else
                {
                    if (rigidbodyDictionary.ContainsKey(otherRigidbody))
                    {
                        rigidbodyDictionary[otherRigidbody].Remove(other);

                        if (rigidbodyDictionary[otherRigidbody].Count == 0)
                        {
                            rigidbodyDictionary.Remove(otherRigidbody);
                            hasExited = true;
                        }
                    }
                }

                detectee = otherRigidbody.GetComponent<TriggerDetectee>();

                if (detectee != null)
                {
                    if (hasEntered && isEnter)
                    {
                        detectee.NotifyTriggerEnter(this, gameObject);
                        FillData(data, detectee);
                        e.Invoke(data);
                    }
                    else if(hasExited && !isEnter)
                    {
                        detectee.NotifyTriggerExit(this, gameObject);
                        FillData(data, detectee);
                        e.Invoke(data);
                    }
                    return;
                }


            }
        }
        
    }
}


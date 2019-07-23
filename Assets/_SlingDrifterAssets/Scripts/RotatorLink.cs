using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Khalreon.Attributes;

public class RotatorLink : MonoBehaviour
{
    [SerializeField]
    private Transform rotatorPoint;

    [Header("Set on runtime")]

    [SerializeField]
    [Disable]
    private Rotator rotator;
    public Rotator Rotator
    {
        set
        {
            rotator = value;
        }
        get
        {
            return rotator;
        }
    }

    //Called from pool
    public void InstallRotator(Rotator rotator)
    {
        this.rotator = rotator;
        rotator.transform.position = rotatorPoint.transform.position;
        rotator.transform.rotation = rotatorPoint.transform.rotation;
    }
    
    //called when cleanup happens.
    public Rotator DetachRotator()
    {
        Rotator detachedRotator = rotator;
        rotator = null;

        return detachedRotator;
    }
}

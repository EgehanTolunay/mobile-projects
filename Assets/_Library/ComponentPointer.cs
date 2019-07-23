using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentPointer<Type> : MonoBehaviour where Type : Component
{
    [SerializeField]
    private Type pointedComponent;
    public Type PointedComponent
    {
        get { return pointedComponent; }
    }
}

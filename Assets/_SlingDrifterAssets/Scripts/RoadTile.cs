using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoadTile : MonoBehaviour
{
    public enum Type
    {
        Left,
        Right,
        Straight
    }

    [SerializeField]
    private Transform inPoint;
    public Transform InPoint
    {
        get { return inPoint; }
    }

    [SerializeField]
    private Transform outPoint;
    public Transform OutPoint
    {
        get { return outPoint; }
    }

    [SerializeField]
    private Type tileType = Type.Straight;
    public Type TileType
    {
        get { return tileType; }
    }

    private Vector2 mapPosition;
    public Vector2 MapPosition
    {
        get { return mapPosition; }
    }

    [SerializeField]
    private Renderer objectRenderer;

    public bool IsBeingSeenByCamera(Camera camera)
    {
        return ExtensionUtil.IsVisibleFrom(objectRenderer, camera);
    }

    private bool passed;
    public bool Passed
    {
        get { return passed; }
        set { passed = value; }
    }

    public void InitializeAsPoolItem()
    {
        gameObject.SetActive(false);
    }

    public UnityAction OnReturnToPool;

    public void ApplyInstallProcess(Vector3 placedDirection, Vector3 placedPosition, Vector2 mapPosition)
    {
        transform.position = placedPosition;
        transform.rotation = Quaternion.LookRotation(placedDirection);
        this.mapPosition = mapPosition;
        gameObject.SetActive(true);
    }

    public void ApplyReturnProcess()
    {
        if(OnReturnToPool != null)
        {
            OnReturnToPool.Invoke();
        }
        gameObject.SetActive(false);
        passed = false;
    }

   
}

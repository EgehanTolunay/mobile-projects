using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        if (followX)
        {
            newPos.x = target.position.x;
        }
        if (followY)
        {
            newPos.y = target.position.y;
        }
        if (followZ)
        {
            newPos.z = target.position.z;
        }

        transform.position = newPos;
    }
}

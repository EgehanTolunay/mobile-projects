using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingCollisionDetection : MonoBehaviour
{
    [SerializeField]
    private Pickup master;
    public Pickup Master
    {
        get
        {
            return master;
        }
        set
        {
            master = value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        master.NotifyHit();
    }
}

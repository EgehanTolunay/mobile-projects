using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControl : MonoBehaviour
{
    private void Awake()
    {
        Input.multiTouchEnabled = false;
    }

    private float horizontalInput;
    public float HorizontalInput
    {
        get { return horizontalInput; }
    }

    void Update()
    {
#if UNITY_EDITOR

        horizontalInput = Input.GetAxisRaw("Horizontal") * 4;
#else
        if (Input.touches.Length == 0){ horizontalInput = 0; return;}

        Touch touch = Input.GetTouch(0);

        horizontalInput = touch.deltaPosition.x;
#endif


    }
}
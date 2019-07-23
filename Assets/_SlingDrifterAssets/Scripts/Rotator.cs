using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    private float clockwiseValue;
    public float ClockwiseValue
    {
        get { return clockwiseValue; }
    }

    [SerializeField]
    private float angularSpeedFactor = 1;

    public void InitializeAsPoolItem()
    {
        gameObject.SetActive(false);
    }

    public void ApplyInstallProcess(float clockwiseValue)
    {
        gameObject.SetActive(true);
        this.clockwiseValue = clockwiseValue;
    }

    public void ApplyReturnProcess()
    {
        gameObject.SetActive(false);
    }

    public Vector3 GetRotatedPosition(Vector3 carPosition, float carSpeed, out Vector3 newDirection)
    {
        Vector3 toCarDirection = carPosition - transform.position;
        float distance = Vector3.Distance(carPosition, transform.position);

        toCarDirection.Normalize();

        Vector3 rotatedDirection = Quaternion.AngleAxis(angularSpeedFactor * (carSpeed * 1.5f) * clockwiseValue * 40 * (1 + Mathf.Clamp01(1-distance)), Vector3.up) * toCarDirection;
        

        Vector3 newPosition = rotatedDirection * distance + transform.position;

        newDirection = (newPosition - transform.position).normalized;

        Debug.DrawLine(transform.position, transform.position + rotatedDirection * distance, Color.red);

        Debug.DrawLine(transform.position, transform.position + toCarDirection * distance, Color.yellow);
        

        return newPosition;
    }
    
}

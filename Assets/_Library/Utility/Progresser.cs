using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Khalreon.Attributes;

[System.Serializable]
public class Progresser
{
    [SerializeField]
    private float currentValue;
    public float CurrentValue { get { return currentValue; } }

    [SerializeField]
    private float maxValue;
    public float MaxValue { get { return maxValue; } }

    public float Percentage
    {
        get
        {
            percentage = currentValue/maxValue;
            return currentValue / maxValue;
        }
        set
        {
            percentage = value;
            currentValue = value * maxValue;
        }
    }
    [SerializeField][NumericLabel]
    private float percentage;

    public Progresser()
    {
        this.maxValue = 1;
    }

    public Progresser(float maxValue)
    {
        this.maxValue = maxValue;
    }

    /// <summary>
    /// Ticks by timeScale*deltaTime.
    /// </summary>
    /// <param name="value"></param>
    public void IncreaseProgress(float value)
    {
        currentValue += value;

        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }
    }

    /// <summary>
    /// Goes backward from current progress by timeScale.
    /// </summary>
    /// <param name="value"></param>
    public void DecreaseProgress(float value)
    {
        currentValue -= value;

        if (currentValue < 0)
        {
            currentValue = 0;
        }
    }

}

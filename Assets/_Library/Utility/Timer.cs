using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Timer{

    [SerializeField]
    private float elapsedTime;
    public float ElapsedTime
    {
        get
        {
            return elapsedTime;
        }
        set
        {
            if(value < 0)
            {
                value = 0;
            }

            elapsedTime = value;

            if(value > duration)
            {
                elapsedTime = duration;
                isFinished = true;
            }

            if (elapsedTime < duration)
            {
                isFinished = false;
            }
        }
    }

    [SerializeField]
    private float duration;
    public float Duration {
        get { return duration; }
        set
        {
            if(value < 0)
            {
                value = 0;
            }

            if (value < elapsedTime)
            {
                elapsedTime = value;
                isFinished = true;
            }
            duration = value;

            if(elapsedTime < duration)
            {
                isFinished = false;
            }
        }
    }

    private bool isFinished;
    public bool IsFinished
    {
        get { return isFinished; }
    }

    public float Percentage
    {
        get
        {
            return elapsedTime / duration;
        }
        set
        {
            elapsedTime = duration * value;
        }
    }

    public float InversePercentage
    {
        get
        {
            return (1 - Percentage);
        }
        set
        {
            Percentage = 1 - value;
        }
    }

    public Timer()
    {
        this.duration = 1;
        Reset();
    }

    public Timer(float duration)
    {
        this.duration = duration;
        Reset();
    }

    public void Reset()
    {
        elapsedTime = 0;
        isFinished = false;
    }

    public void Tick()
    {
        Tick(1);
    }

    /// <summary>
    /// Ticks by timeScale*deltaTime.
    /// </summary>
    /// <param name="timeScale"></param>
    public void Tick(float timeScale)
    {
        Tick(Time.deltaTime, timeScale);
    }

    public void Tick(float deltaTime, float timeScale)
    {
        elapsedTime += deltaTime * timeScale;

        if (elapsedTime > duration)
        {
            elapsedTime = duration;
            isFinished = true;
        }
    }

    /// <summary>
    /// Goes backward from current progress by timeScale.
    /// </summary>
    /// <param name="timeScale"></param>
    public void Rewind(float timeScale)
    {
        elapsedTime -= Time.deltaTime * timeScale;

        if (elapsedTime < 0)
        {
            elapsedTime = 0;
        }
    }
	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVisibilitySetter
{
    void SetVisibility(float percentage);
}

public interface IVisibilityOperator
{
    void ToggleVisibility();
    void Show();
    void Hide();
    void SetVisible();
    void SetHidden();
    void CompleteCurrent();

    bool IsStopped
    {
        get;
        set;
    }

    bool VisibleAtStart
    {
        get;
        set;
    }

    bool IsCompletelyVisible
    {
        get;
    }

    bool IsCompletelyHidden
    {
        get;
    }

    bool IsAppearing
    {
        get;
    }

    bool IsDisappearing
    {
        get;
    }

    bool IsDone
    {
        get;
    }

    float VisibilityPercentage
    {
        get;
        set;
    }

    float ShowDuration
    {
        get;
        set;
    }

    float ElapsedShowTime
    {
        get;
        set;
    }

    float HideDuration
    {
        get;
        set;
    }

    float ElapsedHideTime
    {
        get;
        set;
    }

    AnimationCurve ShowCurve
    {
        get;
        set;
    }

    AnimationCurve HideCurve
    {
        get;
        set;
    }
}

[System.Serializable]
public class VisibilityOperator : IVisibilityOperator
{
    public enum State
    {
        Visible,
        Hiding,
        Appearing,
        Hidden,
        RevertingAppear,
        RevertingHiding,
    }
    [SerializeField]
    private State state;
    
    [SerializeField]
    private bool isStopped;
    public bool IsStopped
    {
        get { return isStopped; }
        set
        {
            isStopped = true;
        }
    }
    
    [SerializeField]
    private bool visibleAtStart;
    public bool VisibleAtStart
    {
        get { return visibleAtStart; }
        set
        {
            visibleAtStart = value;
        }
    }

    public bool IsAppearing
    {
        get { return state == State.Appearing || state == State.RevertingHiding; }
    }

    public bool IsDisappearing
    {
        get { return state == State.Hiding || state == State.RevertingAppear; }
    }

    public bool IsDone
    {
        get { return state == State.Visible || state == State.Hidden; }
    }

    public bool IsCompletelyVisible
    {
        get { return state == State.Visible; }
    }

    public bool IsCompletelyHidden
    {
        get { return state == State.Hidden; }
    }

    private bool isActive;
    public bool IsActive
    {
        get { return isActive; }
        private set
        {
            if (isActive != value)
            {
                isActive = value;
                if (onActiveStatusChanged != null)
                    onActiveStatusChanged.Invoke(isActive);
            }
        }
    }

    [HideInInspector]
    public UnityEngine.Events.UnityAction<bool> onActiveStatusChanged;
    [HideInInspector]
    public UnityEngine.Events.UnityAction<float> onOutputChanged;
    [HideInInspector]
    public UnityEngine.Events.UnityAction onFullVisible;
    [HideInInspector]
    public UnityEngine.Events.UnityAction onFullHidden;
    [HideInInspector]
    public UnityEngine.Events.UnityAction onStartAppearing;
    [HideInInspector]
    public UnityEngine.Events.UnityAction onStartHiding;
    [HideInInspector]
    public UnityEngine.Events.UnityAction onBeginVisible;
    [HideInInspector]
    public UnityEngine.Events.UnityAction onBeginHidden;


    private float visibilityPercentage;
    public float VisibilityPercentage
    {
        get { return visibilityPercentage; }
        set
        {
            if (IsAppearFlow)
            {
                float simulatedTime = 0;
                while (value < materializeCurve.Evaluate(simulatedTime))
                {
                    simulatedTime += Time.deltaTime;

                    if (simulatedTime >= 1)
                    {
                        simulatedTime = 1;
                        break;
                    }
                }

                elapsedShowTime = simulatedTime;
            }
            else
            {
                float simulatedTime = 0;
                while (value > dissolveCurve.Evaluate(simulatedTime))
                {
                    simulatedTime += Time.deltaTime;

                    if (simulatedTime >= 1)
                    {
                        simulatedTime = 1;
                        break;
                    }
                }

                elapsedHideTime = simulatedTime;
            }
        }
    }
    
    [SerializeField]
    private float showDuration;
    public float ShowDuration
    {
        get { return showDuration; }
        set
        {
            showDuration = value;
        }
    }
    
    [SerializeField]
    private float hideDuration;
    public float HideDuration
    {
        get { return hideDuration; }
        set
        {
            hideDuration = value;
        }
    }
    
    [SerializeField]
    private AnimationCurve materializeCurve;
    public AnimationCurve ShowCurve
    {
        get { return materializeCurve; }
        set
        {
            materializeCurve = value;
        }
    }
    
    [SerializeField]
    private AnimationCurve dissolveCurve;
    public AnimationCurve HideCurve
    {
        get { return dissolveCurve; }
        set
        {
            dissolveCurve = value;
        }
    }

    [SerializeField]
    private float hiddenValue = 0;
    [SerializeField]
    private float visibleValue = 1;

    private float elapsedShowTime;
    public float ElapsedShowTime
    {
        get { return elapsedShowTime; }
        set
        {
            elapsedShowTime = value;
            if (IsAppearFlow)
            {
                SetOutput();
            }
        }
    }

    private float elapsedHideTime;
    public float ElapsedHideTime
    {
        get { return elapsedHideTime; }
        set
        {
            elapsedHideTime = value;
            if (IsAppearFlow)
            {
                SetOutput();
            }
        }
    }

    public void SetVisible()
    {
        elapsedHideTime = 0;
        elapsedShowTime = showDuration;
        state = State.Visible;
        SetOutput();
    }

    public void SetHidden()
    {
        elapsedShowTime = 0;
        elapsedHideTime = hideDuration;
        state = State.Hidden;
        SetOutput();
    }

    public void CompleteCurrent()
    {
        if (IsAppearFlow)
        {
            elapsedShowTime = showDuration;
        }
        else
        {
            elapsedHideTime = hideDuration;
        }

        SetOutput();
    }

    public void Hide()
    {
        if (state == State.Hiding || state == State.Hidden || state == State.RevertingAppear) return;

        if (state == State.Visible || state == State.RevertingHiding)
        {
            state = State.Hiding;
            elapsedShowTime = 0;
        }
        else if (state == State.Appearing)
        {
            state = State.RevertingAppear;
        }

        if (onStartHiding != null)
            onStartHiding.Invoke();

        IsActive = true;
        isStopped = false;
    }

    public void Show()
    {
        if (state == State.Appearing || state == State.Visible || state == State.RevertingHiding) return;

        if (state == State.Hidden || state == State.RevertingAppear)
        {
            state = State.Appearing;
            elapsedHideTime = 0;
        }
        else if (state == State.Hiding)
        {
            state = State.RevertingHiding;
        }

        if (onStartAppearing != null)
            onStartAppearing.Invoke();

        IsActive = true;
        isStopped = false;
    }

    public void ToggleVisibility()
    {
        if (state == State.Appearing || state == State.Visible || state == State.RevertingHiding)
        {
            Hide();
        }
        else if (state == State.Hiding || state == State.Hidden || state == State.RevertingAppear)
        {
            Show();
        }
    }

    bool IsAppearFlow
    {
        get
        {
            return state == State.Appearing || state == State.Visible || state == State.RevertingAppear;
        }
    }

    private void SetOutput()
    {
        if (IsAppearFlow)
        {
            visibilityPercentage = Mathf.Lerp(hiddenValue, visibleValue, materializeCurve.Evaluate(elapsedShowTime / showDuration));
        }
        else
        {
            visibilityPercentage = Mathf.Lerp(visibleValue, hiddenValue, dissolveCurve.Evaluate(elapsedHideTime / hideDuration));
        }

        if (onOutputChanged != null)
            onOutputChanged.Invoke(visibilityPercentage);
    }

    public void Begin()
    {
        if (visibleAtStart)
        {
            SetVisible();
            if (onBeginVisible != null)
                onBeginVisible.Invoke();
        }
        else
        {
            SetHidden();
            if (onBeginHidden != null)
                onBeginHidden.Invoke();
        }
    }

    public bool Update()
    {
        if (!isActive) return false;

        if (isStopped)
        {
            IsActive = false;
            return false;
        }

        if (state != State.Hidden && state != State.Visible)
        {
            if (IsAppearFlow)
            {
                if (state == State.RevertingAppear)
                {
                    elapsedShowTime -= Time.deltaTime;

                    if (elapsedShowTime <= 0)
                    {
                        elapsedShowTime = 0;
                    }
                }
                else if (state == State.Appearing)
                {
                    elapsedShowTime += Time.deltaTime;

                    if (elapsedShowTime >= showDuration)
                    {
                        elapsedShowTime = showDuration;
                    }
                }
            }
            else
            {
                if (state == State.RevertingHiding)
                {
                    elapsedHideTime -= Time.deltaTime;

                    if (elapsedHideTime <= 0)
                    {
                        elapsedHideTime = 0;
                    }
                }
                else if (state == State.Hiding)
                {
                    elapsedHideTime += Time.deltaTime;

                    if (elapsedHideTime >= hideDuration)
                    {
                        elapsedHideTime = hideDuration;
                    }
                }
            }

            SetOutput();

            if (visibilityPercentage == hiddenValue)
            {
                state = State.Hidden;
                if (onFullHidden != null)
                    onFullHidden.Invoke();
            }
            else if (visibilityPercentage == visibleValue)
            {
                state = State.Visible;
                if (onFullVisible != null)
                    onFullVisible.Invoke();
            }

            return true;
        }
        else
        {
            isStopped = true;
            return false;
        }

    }
}
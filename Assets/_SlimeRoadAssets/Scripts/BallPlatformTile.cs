using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallPlatformTile : MonoBehaviour, 
    IPoolInitialized<bool>, 
    IPoolInstallProcesser, 
    IPoolReturnProcesser
{
    [SerializeField]
    private int type = 0;
    public int Type
    {
        get { return type; }
    }

    [SerializeField]
    private VisibilityOperator scaleOperator;

    public UnityAction<BallPlatformTile> OnFullHidden;
    public UnityAction<BallPlatformTile> OnFullVisible;

    private void FrameUpdate()
    {
        scaleOperator.Update();

        transform.localScale = new Vector3(scaleOperator.VisibilityPercentage, 1, 1);
    }

    private void OnEnable()
    {
        MonoUpdater.Subscribe(this, FrameUpdate);
    }

    private void OnDisable()
    {
        MonoUpdater.Unsubscribe(this);
    }

    public void ApplyInstallProcess()
    {
        gameObject.SetActive(true);
        scaleOperator.Show();
    }

    public void ApplyReturnProcess()
    {
        scaleOperator.Hide();
    }
    public void InitializeAsPoolItem(bool arg)
    {
        scaleOperator.VisibleAtStart = arg;

        scaleOperator.Begin();

        scaleOperator.Update();

        gameObject.SetActive(false);

        scaleOperator.onFullVisible += (() => { if (OnFullVisible != null) OnFullVisible.Invoke(this); });
        scaleOperator.onFullHidden += (() => 
        {
            if (OnFullHidden != null)
                OnFullHidden.Invoke(this);

            gameObject.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        scaleOperator.onFullVisible -= (() => { if (OnFullVisible != null) OnFullVisible.Invoke(this); });
        scaleOperator.onFullHidden -= (() => 
        {
            if (OnFullHidden != null)
                OnFullHidden.Invoke(this);

            gameObject.SetActive(false);
        });
    }
}

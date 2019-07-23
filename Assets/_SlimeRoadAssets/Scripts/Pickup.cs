using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour,
    IPoolInitialized, 
    IPoolInstallProcesser, 
    IPoolReturnProcesser
{
    public int point;

    public bool isPickedUp;

    public bool canBePickedUp;

    [SerializeField]
    private Transform pickupTransform;

    [SerializeField]
    private Transform obstacleTransform;

    [SerializeField]
    private Renderer obstacleRenderer;

    [SerializeField]
    private VisibilityOperator pickupVisibility;

    [SerializeField]
    private VisibilityOperator obstacleVisibility;

    public UnityAction<Pickup> OnCompletelyVisible;
    public UnityAction<Pickup> OnCompletelyHidden;

    [SerializeField]
    private Material defaultMaterial;

    [SerializeField]
    private Material pickedUpMaterial;

    [SerializeField]
    private Material hitMaterial;

    [SerializeField]
    private List<RingCollisionDetection> collisionDetectors; 

    public void NotifySuccessfulPickup()
    {
        if (!isPickedUp)
        {
            isPickedUp = true;

            obstacleRenderer.material = pickedUpMaterial;
        }
    }

    public void NotifyHit()
    {
        obstacleRenderer.material = hitMaterial;
        GameController.Instance.EndGame();
    }

    public void FrameUpdate()
    {
        pickupVisibility.Update();
        obstacleVisibility.Update();

        if (pickupVisibility.IsCompletelyVisible && obstacleVisibility.IsCompletelyVisible)
        {
            canBePickedUp = true;
        }

        pickupTransform.localScale = new Vector3(pickupVisibility.VisibilityPercentage, pickupVisibility.VisibilityPercentage, pickupVisibility.VisibilityPercentage);
        obstacleTransform.localScale = new Vector3(obstacleVisibility.VisibilityPercentage, obstacleVisibility.VisibilityPercentage, obstacleVisibility.VisibilityPercentage);

    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        collisionDetectors = new List<RingCollisionDetection>(GetComponentsInChildren<RingCollisionDetection>(true));
        
        foreach (RingCollisionDetection col in collisionDetectors)
        {
            col.Master = this;
        }
    }

    public void InitializeAsPoolItem()
    {
        pickupVisibility.Begin();
        obstacleVisibility.Begin();

        pickupVisibility.Update();
        obstacleVisibility.Update();

        pickupVisibility.onFullVisible += NotifyFullVisible;
        obstacleVisibility.onFullVisible += NotifyFullVisible;

        pickupVisibility.onFullHidden += NotifyFullHidden;
        obstacleVisibility.onFullHidden += NotifyFullHidden;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        MonoUpdater.Subscribe(this, FrameUpdate);
    }

    private void OnDisable()
    {
        MonoUpdater.Unsubscribe(this);
    }

    private void OnDestroy()
    {
        pickupVisibility.onFullVisible -= NotifyFullVisible;
        obstacleVisibility.onFullVisible -= NotifyFullVisible;

        pickupVisibility.onFullHidden -= NotifyFullVisible;
        obstacleVisibility.onFullHidden -= NotifyFullVisible;
    }

    private void NotifyFullVisible()
    {
        if(pickupVisibility.IsCompletelyVisible && obstacleVisibility.IsCompletelyVisible)
        {
            canBePickedUp = true;

            if (OnCompletelyVisible != null)
            OnCompletelyVisible.Invoke(this);
        }
    }

    private void NotifyFullHidden()
    {
        if (pickupVisibility.IsCompletelyHidden && obstacleVisibility.IsCompletelyHidden)
        {
            if (OnCompletelyHidden != null)
                OnCompletelyHidden.Invoke(this);

            gameObject.SetActive(false);
        }
    }

    public void ApplyInstallProcess()
    {
        obstacleRenderer.material = defaultMaterial;
        isPickedUp = false;

        gameObject.SetActive(true);

        pickupVisibility.Show();
        obstacleVisibility.Show();
    }

    public void ApplyReturnProcess()
    {
        pickupVisibility.Hide();
        obstacleVisibility.Hide();
    }
}

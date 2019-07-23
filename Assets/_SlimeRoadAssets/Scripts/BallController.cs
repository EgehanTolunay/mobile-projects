using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Khalreon.Attributes;

public class BallController : Singleton<BallController>
{
    [SerializeField]
    private TouchControl touchControl;

    [SerializeField]
    private float ballRadius = 0.7f;

    [SerializeField]
    private float minHeight = 0;

    [SerializeField]
    private float maxHeight = 1;

    [SerializeField][Range(1, 10)]
    private float jumpsPerSecond = 5;

    [SerializeField]
    private AnimationCurve jumpCurve;

    private Timer timer;

    [SerializeField][Range(0,10)]
    private float forwardSpeedMultiplier = 1;

    [SerializeField] [Range(0, 10)]
    private float horizontalSpeedMultiplier = 1;

    [SerializeField][NumericLabel]
    private float forwardPosition;

    [SerializeField][NumericLabel]
    private float horizontalPosition;

    private new Rigidbody rigidbody;

    [SerializeField][NumericLabel]
    private float sincePastLand;

    private int tick;

    public static float MaxBallHeight
    {
        get { return Instance.maxHeight + Instance.ballRadius/2 + 0.23104475f; }
    }

    private float moveStep
    {
        get { return forwardSpeedMultiplier; }
    }

    public static float MoveStep
    {
        get
        {
            return Instance.moveStep;
        }
    }

    public static float ForwardPosition
    {
        get
        {
            return Instance.forwardPosition;
        }
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        timer = new Timer(1 / jumpsPerSecond);

        GameController.Instance.OnGameEnded.AddListener(() => enabled = false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(GameController.Instance != null)
        GameController.Instance.OnGameEnded.RemoveListener(() => enabled = false);
    }

    private float fallback;

    private void Update()
    {
        horizontalPosition += touchControl.HorizontalInput * Time.deltaTime / 3;

        forwardPosition = Mathf.Lerp((tick - 1) * forwardSpeedMultiplier, tick * forwardSpeedMultiplier, timer.Percentage);
        sincePastLand += Mathf.Lerp((tick - 1) * forwardSpeedMultiplier, tick * forwardSpeedMultiplier, timer.Percentage); ;

        timer.Tick();
        float upPosition;
        upPosition = Mathf.Lerp(minHeight + ballRadius/2 + 0.23104475f, maxHeight + ballRadius/2 + 0.23104475f, jumpCurve.Evaluate(timer.Percentage));
        //upPosition = jumpCurve.Evaluate(timer.Percentage) * (maxHeight + ballRadius / 2 + 0.23104475f) + minHeight + ballRadius / 2 + 0.23104475f;

        transform.position = new Vector3(horizontalPosition, upPosition, forwardPosition);

        if (timer.IsFinished)
        {
            timer.Reset();
            timer.Duration = 1 / jumpsPerSecond;

            sincePastLand = 0;
            tick += 1;
            fallback = forwardSpeedMultiplier * jumpsPerSecond * tick - forwardPosition;
        }

        //forwardPosition = Mathf.Lerp(forwardPosition, forwardPosition + fallback, Time.deltaTime);

        transform.position = new Vector3(horizontalPosition, upPosition, forwardPosition);
    }

    private void LateUpdate()
    {
        if(Mathf.Abs(horizontalPosition) > 1.5f)
        {
            horizontalPosition = 1.5f * Mathf.Sign(horizontalPosition);
            transform.position = new Vector3(horizontalPosition, transform.position.y, transform.position.z);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ComponentPointer<Pickup> pointer = other.GetComponent<ComponentPointer<Pickup>>();

        if (pointer == null) return;

        Pickup pickup = pointer.PointedComponent;

        if(pickup != null)
        {
            pickup.NotifySuccessfulPickup();
        }
    }
}

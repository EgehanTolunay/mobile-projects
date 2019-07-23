using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Khalreon.Attributes;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    [SerializeField]
    private LineRenderer line;

    [SerializeField]
    private Transform carTransform;

    [SerializeField][Disable]
    private Rotator connectedRotator;

    [SerializeField]
    private List<Rotator> availableRotators;
    private Dictionary<Rotator, HashSet<RotatorLink>> foundRotators = new Dictionary<Rotator, HashSet<RotatorLink>>();

    private new Rigidbody rigidbody;

    [SerializeField]
    private float speedMultiplier = 5;

    private float driftPenalty;

    [SerializeField][BoolLabel]
    private bool isDrifting;

    private Vector3 desiredDirection;

    private Vector3 driftDirection;

    private RoadTile currentTile;

    [SerializeField]
    private Transform carImageTransform;

    private float carSpeed
    {
        get { return speedMultiplier * Time.deltaTime; }
    }

    [SerializeField]
    private AnimationCurve skidCurve;
    private Timer skidTimer;

    [SerializeField]
    private float skidPower = 5;

    private Quaternion driftRotation;

    private float driftDuration = 0;

    private float driftSign = 0;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        line.positionCount = 2;

        line.gameObject.SetActive(false);

        GameController.Instance.OnGameEnded.AddListener(() => enabled = false);

        skidTimer = new Timer(1);
        skidTimer.Percentage = 1;
    }

    private void OnDestroy()
    {
        GameController.Instance.OnGameEnded.RemoveListener(() => enabled = false);
    }


    private void OnTriggerEnter(Collider other)
    {
        ComponentPointer<RoadTile> pointer = other.GetComponent<ComponentPointer<RoadTile>>();

        if (pointer != null)
        {
            if(pointer.PointedComponent.TileType != RoadTile.Type.Straight)
            {
                RotatorLink link = pointer.PointedComponent.GetComponent<RotatorLink>();
                if (link != null)
                {
                    if (!foundRotators.ContainsKey(link.Rotator))
                    {
                        availableRotators.Add(link.Rotator);
                    }
                    foundRotators.AddOrCreate(link.Rotator, link);
                    
                }
                   
            }

            currentTile = pointer.PointedComponent;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ComponentPointer<RoadTile> pointer = other.GetComponent<ComponentPointer<RoadTile>>();

        if (pointer != null)
        {
            if (pointer.PointedComponent.TileType != RoadTile.Type.Straight)
            {
                RotatorLink link = pointer.PointedComponent.GetComponent<RotatorLink>();
                if (link != null)
                {
                    foundRotators.DecreaseOrRemove(link.Rotator, link);
                    if (!foundRotators.ContainsKey(link.Rotator))
                    {
                        availableRotators.Remove(link.Rotator);
                    }
                }
            }

            pointer.PointedComponent.Passed = true;
        }
    }

    bool wasTouching;

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConnectToRotator();
        }
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            DetachFromRotator();
        }
#else

        if(Input.touches.Length > 0)
        {
            if (!wasTouching)
            {
                wasTouching = true;
                ConnectToRotator();
            }   
        }
        else
        {
            if (wasTouching)
            {
                wasTouching = false;
                DetachFromRotator();
            }
        }
#endif
        
        UpdateMovement();
        
    }

    private void ConnectToRotator()
    {
        Rotator selected = GetClosestRotator();
        if(selected != null)
        {
            connectedRotator = selected;
            isDrifting = true;
            line.gameObject.SetActive(true);
            driftDuration = 0;
            skidTimer.Reset();
        }
    }

    private void DetachFromRotator()
    {
        if (connectedRotator == null) return;

        isDrifting = false;
        line.gameObject.SetActive(false);

        driftSign = Mathf.Sign(connectedRotator.ClockwiseValue);

        skidTimer.Duration = driftDuration + 0.5f;

        driftRotation = carTransform.rotation;


        if (connectedRotator)
        connectedRotator = null;
    }

    private void UpdateMovement()
    {
        //With Rotator you gain drifting, Then you start to lose that without Rotator.

        //Drifting effects your movementDirection. not desiredDirection. 

        desiredDirection = transform.forward;

        if (isDrifting)
        {
            float distance = Vector3.Distance(transform.position, connectedRotator.transform.position);
            line.SetPosition(0, carImageTransform.position);
            line.SetPosition(1, connectedRotator.transform.position);

            if (distance < 3f)
            {
                HookedMovement();
            }
            else
            {
                NormalMovement();
            }
        }
        else
        {
            NormalMovement();
        }
    }


    private void NormalMovement()
    {
        skidTimer.Tick();

        carTransform.rotation = Quaternion.AngleAxis(skidTimer.Duration*skidCurve.Evaluate(skidTimer.Percentage) * driftSign * skidPower, Vector3.up) * Quaternion.LookRotation(transform.forward);
        

        if (currentTile.TileType == RoadTile.Type.Straight)
        {
            Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, currentTile.InPoint.forward, Mathf.Deg2Rad * 2, Time.deltaTime / 10);
            rigidbody.MoveRotation(Quaternion.LookRotation(rotationDirection));
        }

        Vector3 rotatedDirection = desiredDirection + carTransform.forward * 0.5f;

        rigidbody.MovePosition(rotatedDirection * speedMultiplier * Time.deltaTime + transform.position);
    }

    private void HookedMovement()
    {
        driftDuration += Time.deltaTime;

        Quaternion baseRotation = Quaternion.Lerp(carTransform.rotation, Quaternion.AngleAxis(1 * connectedRotator.ClockwiseValue, Vector3.up), driftDuration / 0.2f);

        carTransform.rotation = Quaternion.AngleAxis(driftDuration * connectedRotator.ClockwiseValue * skidPower, Vector3.up) * Quaternion.LookRotation(transform.forward);

        Vector3 newDir;

        Vector3 newPosition = transform.position + (connectedRotator.GetRotatedPosition(transform.position, carSpeed, out newDir) - transform.position).normalized * carSpeed * 1.5f;

        newPosition.y = 0;
        
        Vector3 overRotatedNewDir = Quaternion.AngleAxis(connectedRotator.ClockwiseValue * 150, carTransform.up) * newDir;

        Quaternion rot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(overRotatedNewDir).normalized, Time.deltaTime * 4);

        rigidbody.MovePosition(newPosition);
        
        rigidbody.MoveRotation(rot);
    }

    private Rotator GetClosestRotator()
    {
        Rotator closestRotator = null;
        float closestDistance = 999;
        foreach(Rotator rotator in foundRotators.Keys)
        {
            float distance = Vector3.Distance(rotator.transform.position, transform.position);

            if(closestDistance > distance)
            {
                closestRotator = rotator;
            }
        }

        return closestRotator;
    }

    private Rotator GetLatestRotator()
    {
        if(availableRotators.Count == 0)
        {
            return null;
        }

        return availableRotators[availableRotators.Count - 1];
    }
}

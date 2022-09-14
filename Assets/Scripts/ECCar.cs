using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ECCar : MonoBehaviour
{
    internal Transform thisTransform;

    // static ECCGameController gameController;
    internal Vector3 targetPosition;
    internal RaycastHit groundHitInfo;
    internal Vector3 forwardPoint;
    internal float rightAngle;
    internal Transform healthBar;
    internal float hurtDelayCount = 0;

    [Tooltip("The speed of the player, how fast it moves player. The player moves forward constantly")]
    public float speed = 10;

    [Tooltip("How quickly the player car rotates, in both directions")]
    public float rotateSpeed = 200;

    internal float currentRotation = 0;

    [Tooltip("The slight extra rotation that happens to the car as it turns, giving a drifting effect")]
    public float driftAngle = 50;

    [Tooltip(
        "The slight side tilt that happens to the car chassis as the car turns, making it lean inwards or outwards from the center of rotation")]
    public float leanAngle = 10;

    [Tooltip("The chassis object of the car which leans when the car rotates")]
    public Transform chassis;

    [Tooltip(
        "The wheels of the car which rotate based on the speed of the car. The front wheels also rotate in the direction the car is turning")]
    public Transform[] wheels;

    [Tooltip("The front wheels of the car also rotate in the direction the car is turning")]
    public int frontWheels = 2;

    internal int index;

    [Header("AI Car Attributes")]
    [Tooltip("A random value that is added to the base speed of the AI car, to make their movements more varied")]
    public float speedVariation = 2;

    // The angle range that AI cars try to chase the player at. So for example if 0 they will target the player exactly, while at 30 angle they stop rotating when they are at a 30 angle relative to the player
    [SerializeField] public float chaseAngle;

    [Tooltip("A random value that is to the chase angle to make the AI cars more varied in how to chase the player")]
    public Vector2 chaseAngleRange = new Vector2(0, 30);

    [Tooltip(
        "Make AI cars try to avoid obstacles. Obstacle are objects that have the ECCObstacle component attached to them")]
    public bool avoidObstacles = true;

    [Tooltip("The width of the obstacle detection area for this AI car")]
    public float detectAngle = 2;

    [Tooltip("The forward distance of the obstacle detection area for this AI car")]
    public float detectDistance = 3;

    static Transform targetPlayerTransform;
    [HideInInspector] private Transform groundObject;
    public LayerMask groundLayer;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;
    private bool shouldStopRotating = false;
    private bool shouldScaleUp = false;
    Vector3 velocity;
    public AudioSource tireScreechSound;
    public AudioSource carEngineSound; 

    public GameObject targetToMoveAround;

    public float tenSec = 5;
    public bool timerRunning = true;
    int i;

    private void Start()
    {
        thisTransform = this.transform;

        targetPlayerTransform = targetToMoveAround.transform;

        RaycastHit hit;

        groundObject = GameObject.Find("Floor").transform;

        if (Physics.Raycast(thisTransform.position + Vector3.up * 5 + thisTransform.forward * 1.0f, -10 * Vector3.up,
            out hit, 100, groundLayer)) forwardPoint = hit.point;

        thisTransform.Find("Body").LookAt(forwardPoint);

        // Set a random chase angle for the AI car
        chaseAngle = Random.Range(chaseAngleRange.x, chaseAngleRange.y);
        
        tireScreechSound.Play();
        
        StartCoroutine(ExampleCoroutine());
    }

    IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        shouldStopRotating = true;
    }

    // This function runs whenever we change a value in the component
    private void OnValidate()
    {
        // Limit the maximum number of front wheels to the actual front wheels we have
        frontWheels = Mathf.Clamp(frontWheels, 0, wheels.Length);
    }
    
    // Update is called once per frame
    void Update()
    {
  
        if (shouldStopRotating && !shouldScaleUp)
        {
            var targetRotation = Quaternion.LookRotation(targetPosition - thisTransform.position);
            thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetRotation, speed*20 * Time.deltaTime);
            
            RLWParticleSystem.Stop();
            RRWParticleSystem.Stop();
        
            thisTransform.position = Vector3.SmoothDamp(thisTransform.position, targetPosition, ref velocity, 0.5f, speed);

            if (Vector3.Distance(thisTransform.position, targetPosition) < 0.03f)
            {
                shouldScaleUp = true;
                return;
            }
            
            return;
        };

        if (shouldScaleUp)
        {
            if(tireScreechSound != null && tireScreechSound.isPlaying){
                tireScreechSound.Stop();
            }

            if (carEngineSound != null && carEngineSound.isPlaying)
            {
                carEngineSound.Stop();
            }
            
            transform.localScale = Vector3.Lerp (transform.localScale, new Vector3(0.05f, 0.05f, 0.05f), 2 * Time.deltaTime);

            Transform textTransform = transform.Find(name + "Text");
                
            if (textTransform)
            {
                if (textTransform.gameObject.active)
                {
                    textTransform.gameObject.SetActive(true);
                }
            }
            return;
        }

        // Move the player forward
        thisTransform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);

        // // Get the current position of the target player
            if (targetPlayerTransform) targetPosition = targetPlayerTransform.transform.position;

            // Rotate the car until it reaches the desired chase angle from either side of the player
            if (Vector3.Angle(thisTransform.forward, targetPosition - thisTransform.position) > chaseAngle)
            {
                Rotate(ChaseAngle(thisTransform.forward, targetPosition - thisTransform.position, Vector3.up));
            }
            else // Otherwise, stop rotating
            {
                Rotate(0);
            }

            // If we have no ground object assigned, or it is turned off, then cars will use raycast to move along terrain surfaces
            if (groundObject == null || groundObject.gameObject.activeSelf == false) DetectGround();
        
    }


    /// <summary>
    /// Calculates the approach angle of an object towrads another object
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="targetDirection"></param>
    /// <param name="up"></param>
    /// <returns></returns>
    public float ChaseAngle(Vector3 forward, Vector3 targetDirection, Vector3 up)
    {
        // Calculate the approach angle
        float approachAngle = Vector3.Dot(Vector3.Cross(up, forward), targetDirection);

        // If the angle is higher than 0, we approach from the left ( so we must rotate right )
        if (approachAngle > 0f)
        {
            return 1f;
        }
        else if (
            approachAngle <
            0f) //Otherwise, if the angle is lower than 0, we approach from the right ( so we must rotate left )
        {
            return -1f;
        }
        else // Otherwise, we are within the angle range so we don't need to rotate
        {
            return 0f;
        }
    }


    /// <summary>
    /// Rotates the car either left or right, and applies the relevant lean and drift effects
    /// </summary>
    /// <param name="rotateDirection"></param>
    public void Rotate(float rotateDirection)
    {
        //thisTransform.localEulerAngles = new Vector3(Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal).eulerAngles.x, thisTransform.localEulerAngles.y, Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal).eulerAngles.z);

        //thisTransform.rotation = Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal);

        // If the car is rotating either left or right, make it drift and lean in the direction its rotating
        if (rotateDirection != 0)
        {
            //thisTransform.localEulerAngles = Quaternion.FromToRotation(Vector3.up, groundHitInfo.normal).eulerAngles + Vector3.up * currentRotation;

            // Rotate the car based on the control direction
            thisTransform.localEulerAngles += Vector3.up * rotateDirection * rotateSpeed * Time.deltaTime;

            thisTransform.eulerAngles = new Vector3(thisTransform.eulerAngles.x, thisTransform.eulerAngles.y,
                thisTransform.eulerAngles.z);

            //thisTransform.eulerAngles = new Vector3(rightAngle, thisTransform.eulerAngles.y, forwardAngle);

            currentRotation += rotateDirection * rotateSpeed * Time.deltaTime;

            if (currentRotation > 360) currentRotation -= 360;
            //print(forwardAngle);
            // Make the base of the car drift based on the rotation angle
            thisTransform.Find("Body").localEulerAngles = new Vector3(rightAngle,
                Mathf.LerpAngle(thisTransform.Find("Body").localEulerAngles.y,
                    rotateDirection * driftAngle + Mathf.Sin(Time.time * 50) * hurtDelayCount * 50, Time.deltaTime),
                0); 
            //  Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime);

            // Make the chassis lean to the sides based on the rotation angle
            if (chassis)
                chassis.localEulerAngles = Vector3.forward * Mathf.LerpAngle(chassis.localEulerAngles.z,
                    rotateDirection * leanAngle, Time.deltaTime);
            //  Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime);


            RLWParticleSystem.Play();
            RRWParticleSystem.Play();

            // Go through all the wheels making them spin, and make the front wheels turn sideways based on rotation
             
        }
        else // Otherwise, if we are no longer rotating, straighten up the car and front wheels
        {
            // Return the base of the car to its 0 angle
            thisTransform.Find("Body").localEulerAngles = Vector3.up *
                                                          Mathf.LerpAngle(thisTransform.Find("Body").localEulerAngles.y,
                                                              0, Time.deltaTime * 5);

            // Return the chassis to its 0 angle
            if (chassis)
                chassis.localEulerAngles =
                    Vector3.forward *
                    Mathf.LerpAngle(chassis.localEulerAngles.z, 0,
                        Time.deltaTime *
                        5); //  Mathf.LerpAngle(thisTransform.Find("Base").localEulerAngles.y, rotateDirection * driftAngle, Time.deltaTime);

            RLWParticleSystem.Stop();
            RRWParticleSystem.Stop();
   

            // Go through all the wheels making them spin faster than when turning, and return the front wheels to their 0 angle
            for (index = 0; index < wheels.Length; index++)
            {
                // Return the front wheels to their 0 angle
                if (index < frontWheels)
                    wheels[index].localEulerAngles = Vector3.up *
                                                     Mathf.LerpAngle(wheels[index].localEulerAngles.y, 0,
                                                         Time.deltaTime * 5);

                // Spin the wheel faster
                wheels[index].Rotate(Vector3.right * Time.deltaTime * speed * 30, Space.Self);
            }
        }
    }

    /// <summary>
    /// Detects the terrain under the car and aligns it to it
    /// </summary>
    public void DetectGround()
    {
        // Cast a ray to the ground below
        Ray carToGround = new Ray(thisTransform.position + Vector3.up * 10, -Vector3.up * 20);

        // Detect terrain under the car
        if (Physics.Raycast(carToGround, out groundHitInfo, 20, groundLayer))
        {
            //transform.position = new Vector3(transform.position.x, groundHitInfo.point.y, transform.position.z);
        }

        // Set the position of the car along the terrain
        thisTransform.position =
            new Vector3(thisTransform.position.x, groundHitInfo.point.y + 0.1f, thisTransform.position.z);

        RaycastHit hit;

        // Detect a point along the terrain in front of the car, so that we can make the car rotate accordingly
        if (Physics.Raycast(thisTransform.position + Vector3.up * 5 + thisTransform.forward * 1.0f, -10 * Vector3.up,
            out hit, 100, groundLayer))
        {
            forwardPoint = hit.point;
        }
        else if (groundObject && groundObject.gameObject.activeSelf == true)
        {
            forwardPoint = new Vector3(thisTransform.position.x, groundObject.position.y, thisTransform.position.z);
        }

        // Make the car look at the point in front of it along the terrain
        thisTransform.Find("Base").LookAt(forwardPoint);
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position + Vector3.up * 0.2f + transform.right * detectAngle * 0.5f +
            transform.right * detectAngle * 0.0f * Mathf.Sin(Time.time * 50),
            transform.TransformDirection(Vector3.forward) * detectDistance);
        Gizmos.DrawRay(
            transform.position + Vector3.up * 0.2f + transform.right * -detectAngle * 0.5f -
            transform.right * detectAngle * 0.0f * Mathf.Sin(Time.time * 50),
            transform.TransformDirection(Vector3.forward) * detectDistance);

        Gizmos.DrawSphere(forwardPoint, 0.5f);
    }
}
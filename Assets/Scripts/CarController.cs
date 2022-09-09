using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform[] wheels;
    public int frontWheels = 2;
    [SerializeField] private float speed = 100f;

    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;

    public float tenSec = 70;
    public bool timerRunning = true;
    int i;

    private void Start()
    {
    }

    private void Update()
    {
        if (timerRunning)
        {
            tenSec -= Time.smoothDeltaTime;
            if (tenSec >= 0)
            {
                // if (tenSec <= 69)
                // {
                //     Debug.Log("Drifting rear wheels");
                //     SpinRearWheels();
                // }
                //
                // if (tenSec <= 68)
                // {
                //     RLWParticleSystem.Play();
                //     RRWParticleSystem.Play();
                // }

                if (tenSec <= 69)
                {
                    speed = 0.1f;
                    this.transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);

                    this.transform.Find("Body").parent.localEulerAngles = new Vector3(
                        0,
                        Mathf.LerpAngle(this.transform.Find("Body").localEulerAngles.y,
                            20 * 100f + Mathf.Sin(Time.time * 50) * 200 * 50, Time.deltaTime),
                        0);
                }
            }
            else
            {
                Debug.Log("Done");
                timerRunning = false;
            }
        }
    }

    private void SpinRearWheels()
    {
        int index;
        for (index = 2; index < wheels.Length; index++)
        {
            // Turn the front wheels sideways based on rotation
            // if (index < frontWheels)
            //     wheels[index].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[index].localEulerAngles.y,
            //         rotateDirection * driftAngle, Time.deltaTime * 10);

            // Spin the wheel
            wheels[index].Rotate(Vector3.right * Time.deltaTime * speed * 20, Space.Self);
        }
    }
}
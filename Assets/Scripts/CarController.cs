using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("General")]
    [Space(5)]
    public Transform[] wheels;
    public int frontWheels = 2;
    [SerializeField] 
    private float speed = 100f; 
    [SerializeField] 
    private Transform targetTransform;
    
    [Space(15)]
    [Header("Effects")]
    [Space(5)]
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    
    [Space(15)]
    [Header("Sounds")]
    [Space(5)]
    //The following variable lets you to set up sounds for your car such as the car engine or tire screech sounds.
    public bool useSounds = false;
    public AudioSource carEngineSound; // This variable stores the sound of the car engine.
    public AudioSource tireScreechSound;

    private bool isDrifting = false;
    private float tenSec = 25;
    private int i;
    private float initialCarEngineSoundPitch;
    private Transform thisTransform;
    
    internal Vector3 targetPosition;
    private Vector3 velocity;


    private void Start()
    {

        targetPosition = new Vector3(0, 0, 0);
        thisTransform = this.transform;
            
        if(useSounds){
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }else if(!useSounds){
            if(carEngineSound != null){
                carEngineSound.Stop();
            }
            if(tireScreechSound != null){
                tireScreechSound.Stop();
            }
        }
        
        // We save the initial pitch of the car engine sound.
        if(carEngineSound != null){
            initialCarEngineSoundPitch = carEngineSound.pitch - 0.2f;
        }
    }

    private void Update()
    {
        
            tenSec -= Time.smoothDeltaTime;
            if (tenSec >= 0)
            {
                if (tenSec <= 19)
                {
                    SpinRearWheels();
                    isDrifting = true;
                    carEngineSound.pitch = Mathf.Lerp(0.5f, 1,Time.time); 
                }
                
                if (tenSec <= 18 && tenSec >= 0)
                {
                    RLWParticleSystem.Play();
                    RRWParticleSystem.Play();
                }
                
                if (tenSec <= 15 && tenSec >= 12)
                {
                    ForwardMovement();
                }

                if (tenSec <= 12 && tenSec >= 10)
                {
                    Quaternion rotTarget = Quaternion.LookRotation(targetPosition - transform.position);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, 90*Time.deltaTime);
                }

                if (tenSec <= 10 && tenSec >= 6)
                {
                    DriftAroundOrigin();
                }
                if(tenSec <= 6 )
                {
                    var targetRotation = Quaternion.LookRotation(targetPosition - thisTransform.position);
                        thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetRotation, speed*20 * Time.deltaTime);
            
                        RLWParticleSystem.Stop();
                        RRWParticleSystem.Stop();
                        isDrifting = false;
        
                        thisTransform.position = Vector3.SmoothDamp(thisTransform.position, targetPosition, ref velocity, 0.5f, speed);
                        
                        int index;
                        for (index = 0; index < 2; index++)
                        {
                            // Turn the front wheels sideways based on rotation
                            if (index < frontWheels)
                                wheels[index].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[index].localEulerAngles.y,
                                    0, Time.deltaTime * 10);
                        
                        }
                }
            }
            
        
    }

    private void SpinRearWheels()
    {
        int index;
        for (index = 2; index < wheels.Length; index++)
        {
            // Spin the wheel
            wheels[index].Rotate(Vector3.right * Time.deltaTime * speed * 20, Space.Self);
        }
    }
    
    public void CarSounds(){

        if(useSounds){
            try{
                
                if(isDrifting){
                    if(!tireScreechSound.isPlaying){
                        tireScreechSound.Play();
                    }
                }else {
                    tireScreechSound.Stop();
                }
            }catch(Exception ex){
                Debug.LogWarning(ex);
            }
        }else if(!useSounds){
            if(carEngineSound != null && carEngineSound.isPlaying){
                carEngineSound.Stop();
            }
            if(tireScreechSound != null && tireScreechSound.isPlaying){
                tireScreechSound.Stop();
            }
        }

    }

    public void ForwardMovement()
    {
        speed = 0.1f;
        this.transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
                    
                    
        int inde;
        for (inde = 0; inde < 2; inde++)
        {
            // Turn the front wheels sideways based on rotation
            if (inde < frontWheels)
                wheels[inde].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[inde].localEulerAngles.y,
                    20, Time.deltaTime * 10);
                        
        }

        this.transform.localEulerAngles = new Vector3(0,
            Mathf.LerpAngle(this.transform.localEulerAngles.y,
                40, Time.deltaTime),
            0);

        int index;
        for (index = 0; index < 2; index++)
        {
            // Turn the front wheels sideways based on rotation
            if (index < frontWheels)
                wheels[index].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[index].localEulerAngles.y,
                    -40, Time.deltaTime * 10);
                        
        }
    }
    
    public void DriftAroundOrigin()
    {
        transform.LookAt(new Vector3(0,0,0));
        transform.RotateAround(new Vector3(0,0,0), new Vector3(0,1 ,0) ,90f * Time.deltaTime);
        
        carEngineSound.pitch = Mathf.Lerp(0.7f, 1.1f ,Time.time);
        
        transform.Rotate(0, 30, 0);

        int index;
        
        for (index = 0; index < 2; index++)
        {
            // Turn the front wheels sideways based on rotation
            if (index < frontWheels)
                wheels[index].localEulerAngles = Vector3.up * Mathf.LerpAngle(wheels[index].localEulerAngles.y,
                    -65, Time.deltaTime * 10);
                        
        }
    }
    
}
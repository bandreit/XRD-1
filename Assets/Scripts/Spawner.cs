using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] 
    private GameObject obstacle;
    [SerializeField] 
    private GameObject target;
    [SerializeField] 
    private float speed = 10;

    private float time = 2;
    private void Update()
    {
        time -= Time.smoothDeltaTime;

        if (time == 2)
        {
            var newObject = GameObject.Instantiate(obstacle,  new Vector3(target.transform.position.x,0,target.transform.position.z + 0.5f), Quaternion.identity); 
            newObject.transform.Translate(new Vector3(0,0,-1) * Time.deltaTime);
            Destroy(newObject, 10);
        }
        else
        {
            time = 2;
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleController : MonoBehaviour
{
    // Adjust the speed for the application.
    public float speed = 0.5f;
    private GameObject targetGameObject;
    // The target (cylinder) position.
    private Vector3 target;

    void Awake()
    {
        targetGameObject = GameObject.Find("Mustang");
        if (targetGameObject)
        {
            var carPosition = targetGameObject.transform.position;
            target = new Vector3(transform.position.x, 0, carPosition.z - 1f);
        
            Destroy(gameObject, 5f);
        }
    }

    void Update()
    {
        // var step =  speed * Time.deltaTime; // calculate distance to move
        // transform.position = Vector3.MoveTowards(transform.position, target, step);
    }
}

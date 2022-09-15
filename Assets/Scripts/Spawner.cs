using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] 
    private GameObject obstacle;
    [SerializeField] 
    private GameObject target;

    private Vector3 targetPosition;
    void Start()
    {
        InvokeRepeating("LaunchObstacle", 5.5f, 0.7f);
        if (target)
        {
            targetPosition = target.transform.position; 
        }
    }

    void LaunchObstacle()
    {
        if (target)
        {
            GameObject obst = Instantiate(
                obstacle, 
                new Vector3(
                    Random.Range(targetPosition.x - 0.01f, targetPosition.x + 0.01f), targetPosition.y, targetPosition.z + 0.01f),
                Quaternion.identity);
        }
    }
}

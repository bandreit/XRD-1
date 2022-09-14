using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MyDefaultEventTrackable : DefaultObserverEventHandler {
    protected override void OnTrackingFound()
    {
        if (mObserverBehaviour)
        {
            string[] cars = {"Mustang", "Ferrari", "Camaro"};
            foreach (var car in cars)
            {
                if (transform.Find(car))
                {
                    GameObject carObject = transform.Find(car).gameObject;
                    carObject.SetActive(true);
                }
            }
            
            var rendererComponents = mObserverBehaviour.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = mObserverBehaviour.GetComponentsInChildren<Collider>(true);
            var canvasComponents = mObserverBehaviour.GetComponentsInChildren<Canvas>(true);
            // Enable rendering:
            foreach (var component in rendererComponents)
                component.enabled = true;
            // Enable colliders:
            foreach (var component in colliderComponents)
                component.enabled = true;
            // Enable canvas':
            foreach (var component in canvasComponents)
                component.enabled = true;
        }
        OnTargetFound?.Invoke();
    }
}
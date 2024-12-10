using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class OrbitalCamera : MonoBehaviour
{
    public float upDistance = 8f;
    public float backDistance = 0f;
    public float trackingSpeed = 5f;
    public float rotationSpeed = 9f;
    public Transform target;
    private Vector3 cameraMovement;
    private Quaternion cameraRotation;

    void FixedUpdate()
    {
        if (target != null)
        {
            cameraMovement = target.position - target.forward * backDistance + target.up * upDistance;
            transform.position = Vector3.Lerp(transform.position, cameraMovement, trackingSpeed * Time.deltaTime);
            cameraRotation = Quaternion.LookRotation(-target.up, target.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, cameraRotation, rotationSpeed * Time.deltaTime);
        }
    }

}

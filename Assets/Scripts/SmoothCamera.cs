using System;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    public Vector3 offset;
    
    [Header("Smooth Settings")]
    public float smoothSpeed = 2f;

    private void Start()
    {
        Instantiate(Resources.Load<GameObject>("FadeCanvas"), Vector3.zero, Quaternion.identity);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            FollowTarget();
        }
    }
    void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
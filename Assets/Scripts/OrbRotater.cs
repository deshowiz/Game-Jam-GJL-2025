using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbRotater : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Orb[] _orbs = new Orb[2];
    [SerializeField]
    private TrailRenderer _orbTrail = null;
    [SerializeField]
    private TrailRenderer _orbTrail2 = null;

    public Vector3 _playerPosition = Vector3.zero;

    [Header("Visual Stats")]
    [SerializeField]
    public float _speed = 1f;

    [Header("Settings")]
    
    [SerializeField]
    private float _radius = 1f;
    [SerializeField]
    private float _hitThreshold = 0f;
    [SerializeField]
    private float _disableTiming = 0.3f;

    private bool _isRunning = false;

    public float _currentAngle = 90f;

    private void Awake()
    {
        _orbTrail.GetComponent<TrailRenderer>().material.renderQueue = 4000;
        _orbTrail2.GetComponent<TrailRenderer>().material.renderQueue = 4000;
    }

    public void BeginRunning()
    {
        _isRunning = true;
    }
    public void SetNewRotation(float newRotationAdditive)
    {
        if (!_isRunning) return;
        //Debug.Log(newRotationAngle);
        //_totalTime += Time.deltaTime * _speed;
        //Debug.Log("Total time: " + _totalTime);
        // newRotationAngle = newRotationAngle / 360f;
        //float fullSpeed = _speed + _currentBoost;
        _currentAngle += newRotationAdditive;
        float angleRadians = _currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(angleRadians), Mathf.Cos(angleRadians), 0f) * _radius;
        _orbs[0].transform.localPosition = offset;

        float nextAngle = angleRadians + Mathf.PI;
        offset = new Vector3(Mathf.Sin(nextAngle), Mathf.Cos(nextAngle), 0f) * _radius;
        _orbs[1].transform.localPosition = offset;

        // _orbTrail.time = _trailLength / fullSpeed;
        // _orbTrail2.time = _trailLength / fullSpeed;
    }

    // Bad expensive Game Jam code activate!
    public bool CheckOrbAccuracy(int orbIndex, List<PlayerMovement.RouteData> allInteractables)
    {
        if (_orbs[orbIndex].IsDisabled) return false;

        Transform chosenOrb = _orbs[orbIndex].transform;
        InteractableTile currentInteractable;
        Vector2 orbXZ = new Vector2(chosenOrb.position.x, chosenOrb.position.z);
        for (int i = 0; i < allInteractables.Count; i++)
        {
            currentInteractable = allInteractables[i].interactableTile;
            float accuracyDistance = Vector2.Distance(orbXZ,
             new Vector2(currentInteractable.transform.position.x, currentInteractable.transform.position.z));
            if (accuracyDistance < _hitThreshold)
            {
                Debug.Log("interacting");
                currentInteractable.Interact();
                return true;
            }
        }

        // Timed Disabling of orb?
        _orbs[orbIndex].DisableOrb();
        return false;
    }

    public void SetRadius(float newRadius)
    {
        _radius = newRadius;
    }
}

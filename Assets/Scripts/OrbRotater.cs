using System;
using UnityEngine;

public class OrbRotater : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform _orbTransform = null;
    [SerializeField]
    private Transform _orbTransform2 = null;
    [SerializeField]
    private TrailRenderer _orbTrail = null;
    [SerializeField]
    private TrailRenderer _orbTrail2 = null;

    public Vector3 _playerPosition = Vector3.zero;

    private float _totalTime = 0f;
    [Header("Visual Stats")]
    [SerializeField]
    public float _speed = 1f;

    //private float _currentAngle = 0f;

    private float _currentBoost = 0f;

    [Header("Settings")]

    [SerializeField]
    private float _perfectBoostIncrement = 0.5f;
    [SerializeField]
    [Range(0f, 5f)]
    private float _okayBoostDecrement = 0.5f;
    [SerializeField]
    [Range(0f, 5f)]
    private float _missBoostDecrement = 2f;
    [SerializeField]
    private float _maximumBoost = 15f;
    [SerializeField]
    private float _minimumBoost = -1.5f;
    
    [SerializeField]
    private float _radius = 1f;
    [SerializeField]
    private float _trailLength = 0.5f;
    [SerializeField]
    private float _perfectThreshold = 0f;
    [SerializeField]
    private float _goodThreshold = 0f;
    [SerializeField]
    private float _okayThreshold = 0f;

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
        _orbTransform.localPosition = offset;

        float nextAngle = angleRadians + Mathf.PI;
        offset = new Vector3(Mathf.Sin(nextAngle), Mathf.Cos(nextAngle), 0f) * _radius;
        _orbTransform2.localPosition = offset;

        // _orbTrail.time = _trailLength / fullSpeed;
        // _orbTrail2.time = _trailLength / fullSpeed;
    }

    public int CheckOrbAccuracy(int orbIndex)
    {
        Transform chosenOrb = orbIndex == 0 ? _orbTransform : _orbTransform2;
        Vector3 grabbedNextPos = GameManager.Instance._nextTilePosition;
        float accuracyDistance = Vector2.Distance(new Vector2(chosenOrb.position.x, chosenOrb.position.z), new Vector2(grabbedNextPos.x, grabbedNextPos.z)); // switch out with current orb variable later if double orbing?
        //Debug.Log(accuracyDistance);
        if (accuracyDistance < _perfectThreshold) // Not doing a pre-calc for a switch atm, maybe later
        {
            _currentBoost = Mathf.Min(_currentBoost + _perfectBoostIncrement, _maximumBoost); //Debug.Log("Perfect");
            return 3; // Perfect
        }
        else if (accuracyDistance < _goodThreshold)
        {
            //Debug.Log("Good");
            return 2; //Good
        }
        else if (accuracyDistance < _okayThreshold)
        {
            _currentBoost = Mathf.Max(_currentBoost - _okayBoostDecrement, _minimumBoost);
            //Debug.Log("Okay");
            return 1; // Okay
        }
        else
        {
            //Debug.Log("Miss");
            _currentBoost = Mathf.Max(_currentBoost - _missBoostDecrement, _minimumBoost);
            return 0; // Miss
        }
    }

    public void SetRadius(float newRadius)
    {
        _radius = newRadius;
    }
}

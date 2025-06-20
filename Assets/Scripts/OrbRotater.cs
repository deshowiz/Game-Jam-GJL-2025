using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
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
    private bool _isRunning = false;
    [SerializeField]
    private KeyCode _orb0RecoveryKey = KeyCode.A;
    [SerializeField]
    private KeyCode _orb1RecoveryKey = KeyCode.D;

    public float _currentAngle = 90f;

    Coroutine _slowMoRecovery = null;

    WaitForFixedUpdate _endFrameWaiter = null;

    private void Awake()
    {
        _orbTrail.GetComponent<TrailRenderer>().material.renderQueue = 4000;
        _orbTrail2.GetComponent<TrailRenderer>().material.renderQueue = 4000;
        _endFrameWaiter = new WaitForFixedUpdate();
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

    public void SetOrbSize(float newScale)
    {
        _orbs[0].transform.localScale = new Vector3(newScale, newScale, newScale);
        _orbs[1].transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    // Bad expensive Game Jam code activate!
    public bool CheckOrbAccuracy(int orbIndex, List<PlayerMovement.RouteData> allInteractables)
    {
        if (_orbs[orbIndex].IsDisabled) return false;
        Transform chosenOrb = _orbs[orbIndex].transform;
        float scaledHitThreshold = _hitThreshold * chosenOrb.localScale.x;
        InteractableTile currentInteractable;
        Vector2 orbXZ = new Vector2(chosenOrb.position.x, chosenOrb.position.z);
        for (int i = 0; i < 1; i++)
        {
            currentInteractable = allInteractables[i].interactableTile;
            float accuracyDistance = Vector2.Distance(orbXZ,
             new Vector2(currentInteractable.transform.position.x, currentInteractable.transform.position.z));
            // float yDiff = GameManager.Instance.transform.position.y > currentInteractable.transform.position.y ?
            //     Mathf.Abs(GameManager.Instance.transform.position.y - currentInteractable.transform.position.y) :
            //         Mathf.Abs(currentInteractable.transform.position.y - GameManager.Instance.transform.position.y);
            if (accuracyDistance < scaledHitThreshold/* + yDiff * 0.5f*/)
            {
                currentInteractable.Interact();
                _orbs[orbIndex].GlowOnHit();
                // Orb _firstIndexOrb = _orbs[0];
                // _orbs[0] = _orbs[1];
                // _orbs[1] = _firstIndexOrb;
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

    public void SetOrbHeight(float newY, bool isBlue)
    {
        if (isBlue)
        {
            _orbs[0].transform.position = new Vector3(_orbs[0].transform.position.x, newY, _orbs[0].transform.position.z);
        } else
        {
            _orbs[1].transform.position = new Vector3(_orbs[1].transform.position.x, newY, _orbs[1].transform.position.z);
        }
    }

    public void SlowMoRecovery(int numPresses)
    {
        if (_slowMoRecovery != null)
        {
            StopCoroutine(_slowMoRecovery);
            _slowMoRecovery = StartCoroutine(SlowmoMinigame(numPresses));
            return;
        }
        _slowMoRecovery = StartCoroutine(SlowmoMinigame(numPresses));
    }

    public IEnumerator SlowmoMinigame(int numPresses)
    {
        _orbs[0].FullDisable();
        _orbs[1].FullDisable();
        _orbs[0].SetOrbRecovery(0f);
        _orbs[1].SetOrbRecovery(0f);
        int currentPresses = 0;
        bool updateRecovery = false;
        while (currentPresses < numPresses)
        {
            if (Input.GetKeyDown(_orb0RecoveryKey))
            {
                currentPresses++;
                updateRecovery = true;
            }

            if (Input.GetKeyDown(_orb1RecoveryKey))
            {
                currentPresses++;
                updateRecovery = true;
            }

            if (updateRecovery)
            {
                float recoveryProgress = Mathf.Min((float)currentPresses / numPresses, 1f);
                if (recoveryProgress == 1f) break;
                _orbs[0].SetOrbRecovery(recoveryProgress);
                _orbs[1].SetOrbRecovery(recoveryProgress);
            }

            updateRecovery = false;
            yield return null;
        }
        yield return null;

        _orbs[0].ReEnable();
        _orbs[1].ReEnable();
        Time.timeScale = 1f;
    }

}

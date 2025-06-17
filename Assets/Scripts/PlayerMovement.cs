using System.Collections;
using UnityEngine;
using DigitalRuby.Tween;
using System.Linq;
using System;
using Unity.Burst.Intrinsics;

public class PlayerMovement : MonoBehaviour
{
    public OrbRotater orbRotater;
    public GameEvent OnPlayerStepEvent;

    [SerializeField]
    private GameEvent _OnPlayerFinishTileEvent;

    [Header("Settings")]
    [SerializeField]
    [Range(0f, 10f)]
    private float _baseSpeed = 1f;
    private float _boostSpeed = 0f;
    [SerializeField]
    private float _rotationScalar = 5f;

    private float _stepPercentageCompleted = 0f;

    [Range(0f, 5f)]
    public float movementTweenSpeed = 0.15f;
    private float _fullSpeed = 0f;
    private bool ready;

    private bool _jumping = false;

    private float _currentRouteBaseTiming = 0f;
    private float _incrementedTiming = 0f;
    private float _fullRouteTiming = 0f;

    private float _currentRouteRotation = 0f;

    private float _startRouteRotation = 0f;

    private float _totalDegreesThisRoute = 0f;


    // Use next position in GameManager to get move direction
    // record travel distance between interactables
    // in the beginning, ^the first interactable position will need to be the starting tile position, 0,0,0
    // frame by frame, below \\
    // take that distance and dynamically, divide how long it will take to get there
    // Use that time and take the current speed of the player and divide for number of rotations
    // round down
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Time.timeScale = 1f;
            ready = !ready;
        }

        if (!ready) return;
        _fullSpeed = _baseSpeed + _boostSpeed;
        float distToNext = DistanceToNextTile();
        _incrementedTiming = ((_fullSpeed * Time.deltaTime / _currentRouteBaseTiming) * _totalDegreesThisRoute);
        //_currentRouteBaseTiming
        //_currentRouteRotation

        // Must be additive
        orbRotater.SetNewRotation(_incrementedTiming);


        if (distToNext == 1f) // Distance of greater than 1 indicates a gap since all tiles need to have a diameter of 1 or lower
        {
            Slide();
        }
        else if (!_jumping)/*if (Time.time >= lastStepTime + timeBetweenSteps)*/
        {
            Step();
        }
    }

    public void Slow(float slowPercentage)
    {
        _boostSpeed *= (1f - slowPercentage / 100f);
        _boostSpeed = Mathf.Max(_boostSpeed, 0f);
    }
    
    public void Boost(float boostPercentage)
    {
        _boostSpeed *= (1f + boostPercentage / 100f);
        _boostSpeed = Mathf.Max(_boostSpeed, 0f);
    }

    public IEnumerator Stun(float seconds)
    {
        ready = false;
        yield return new WaitForSeconds(seconds);
        ready = true;
    }

    // Changing the movement to be sliding when not originally intended is a bit scuffed structure wise
    // Getting all of the edge cases for something not originally designed for the sliding was rough
    // Would have a better solution if it didn't require refactoring a previous system set from diff expectations
    private void Slide()
    {

        Vector3 finalDestination = transform.position;
        float distanceThisFrame = _fullSpeed * Time.deltaTime;
        _stepPercentageCompleted += distanceThisFrame;

        if (_stepPercentageCompleted < 1f)
        {
            finalDestination += GameManager.Instance.GetNormalizedDirection() * distanceThisFrame;
        }
        else
        {
            do
            { // In case the player moves more than one tile's space during a frame
                _stepPercentageCompleted--;
                finalDestination = GameManager.Instance.TileIncrement();
                //Time.timeScale = 0f;
                if (GameManager.Instance.GetNextTileInteractable())
                {
                    Debug.Log("grabbing Next");
                    InteractableRouteComplete();
                }
                Vector3 nextTile = GameManager.Instance.GetNextTilePos();
                finalDestination = Vector3.Lerp(finalDestination, nextTile, _stepPercentageCompleted);
            }
            while (_stepPercentageCompleted > 1f);
        }

        transform.position = new Vector3(finalDestination.x, 0f, finalDestination.z);
    }

    private void Step()
    {
        //todo: if stepDistance > 1, check which tile it is
        Vector3 newTargetPosition = GameManager.Instance.GetNextTilePos();
        Debug.Log(Time.time);
        if (newTargetPosition.x == Mathf.NegativeInfinity)
        {
            Debug.LogError("Position not set yet?");
            return;
        }

        Vector3 targetPos = new Vector3(newTargetPosition.x, transform.position.y,
            newTargetPosition.z);

        System.Action<ITween<Vector3>> updateNextTile = (t) =>
        {
            Vector3 nextTile = GameManager.Instance.TileIncrement();
            _jumping = false;
            if (GameManager.Instance.GetNextTileInteractable())
            {
                Debug.Log("grabbing Next");
                InteractableRouteComplete();
            }
        };

        _jumping = true;
        gameObject.Tween("PlayerMove",
            transform.position,
            targetPos,
            movementTweenSpeed / _fullSpeed,
            TweenScaleFunctions.CubicEaseIn,
            (t) => transform.position = t.CurrentValue, updateNextTile
        );

        // lastStepTime += timeBetweenSteps;
        // OnPlayerStepEvent.Raise();
    }

    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePosition = GameManager.Instance.CurrentTilePosition;
        Vector3 nextTilePosition = GameManager.Instance._nextTilePosition;
        return Vector3.Distance(GameManager.Instance.GetCurrentTilePos(),
         GameManager.Instance.GetNextTilePos());
    }

    public void InteractableRouteComplete() // Sent here since it has access to speed for dynamic scaling of rotations
    {
        _incrementedTiming = 0f;
        GameManager.Instance._interactableRouteTimings.RemoveFirst();
        GameManager.RouteData newRouteData = GameManager.Instance._interactableRouteTimings.First();
        Debug.Log(newRouteData.fullTiming);
        _currentRouteBaseTiming = newRouteData.fullTiming;
        // float newAngle = Vector2.Angle(new Vector2(newRouteData.interactableTilePosition.x, newRouteData.interactableTilePosition.z),
        // new Vector2(newRouteData.lastTilePosition.x, newRouteData.lastTilePosition.z));
        Vector3 normalizedDir = (newRouteData.interactableTilePosition - newRouteData.lastTilePosition).normalized;
        float newAngle = Mathf.Atan2(normalizedDir.x, normalizedDir.z) * Mathf.Rad2Deg;
        if (normalizedDir.z < 0) newAngle = 180f - newAngle;
        else if (normalizedDir.x < 0) newAngle = 360f + newAngle;
        _startRouteRotation = orbRotater._currentAngle;

        //Debug.Log(_startRouteRotation);

        float angleDiff = Mathf.DeltaAngle(_startRouteRotation, newAngle);
        float extraCircles = Mathf.Max(Mathf.RoundToInt(_fullRouteTiming / 3f), 1) * 360f * Mathf.Sign(angleDiff);
        _totalDegreesThisRoute = angleDiff + extraCircles;
        //Debug.Log(angleDiff);
        _currentRouteRotation = angleDiff + _startRouteRotation;
        //Debug.Log("New Angle: " + _currentRouteRotation + " >>>>> between interactable " + newRouteData.interactableTilePosition + " and previous " + newRouteData.lastTilePosition);
        orbRotater.SetRadius(Vector3.Distance(newRouteData.interactableTilePosition, newRouteData.lastTilePosition));
        //Time.timeScale = 0f;
    }

    public void StartFirstRoute()
    {
        _incrementedTiming = 0f;
        GameManager.RouteData newRouteData = GameManager.Instance._interactableRouteTimings.First();
        //Debug.Log(newRouteData.fullTiming);
        _currentRouteBaseTiming = newRouteData.fullTiming;
        Debug.Log(newRouteData.fullTiming);
        // float newAngle = Vector2.Angle(new Vector2(newRouteData.interactableTilePosition.x, newRouteData.interactableTilePosition.z),
        // new Vector2(newRouteData.lastTilePosition.x, newRouteData.lastTilePosition.z));
        Vector3 normalizedDir = (newRouteData.interactableTilePosition - newRouteData.lastTilePosition).normalized;
        float newAngle = Mathf.Atan2(normalizedDir.x, normalizedDir.z) * Mathf.Rad2Deg;
        if (normalizedDir.z < 0) newAngle = 180f - newAngle;
        else if (normalizedDir.x < 0) newAngle = 360f + newAngle;
        orbRotater._currentAngle = 90f;
        _startRouteRotation = orbRotater._currentAngle;

        //Debug.Log(_startRouteRotation);

        float angleDiff = Mathf.DeltaAngle(_startRouteRotation, newAngle);
        float extraCircles = Mathf.Max(Mathf.RoundToInt(_fullRouteTiming / 3f), 1) * 360f * Mathf.Sign(angleDiff);
        _totalDegreesThisRoute = angleDiff + extraCircles;
        //Debug.Log(angleDiff);
        _currentRouteRotation = angleDiff + _startRouteRotation;
        //Debug.Log("New Angle: " + _currentRouteRotation + " >>>>> between interactable " + newRouteData.interactableTilePosition + " and previous " + newRouteData.lastTilePosition);
        orbRotater.SetRadius(Vector3.Distance(newRouteData.interactableTilePosition, newRouteData.lastTilePosition));
        //Time.timeScale = 0f;
    }
    
}
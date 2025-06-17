using System.Collections;
using UnityEngine;
using DigitalRuby.Tween;
using System.Linq;
using System;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using System.Collections.Generic;

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
    private void Update()
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
        // if (_pathPositionsToRemove != 0)
        // {
        //     Debug.Log("REMOVING PATH POSITIONS OF NUMBER " + _pathPositionsToRemove);
        //     _playerPathPositions.RemoveRange(0, _pathPositionsToRemove);
        //     _currentlyTravelledIndex -= _pathPositionsToRemove;
        //     _pathPositionsToRemove = 0;
        // }
    }

    private void LateUpdate()
    {
        if (_pathPositionsToAdd.Count > 0)
        {
            _playerPathPositions.AddRange(_pathPositionsToAdd);
            _pathPositionsToAdd.Clear();
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
            finalDestination += GetNormalizedDirection() * distanceThisFrame;
        }
        else
        {
            do
            { // In case the player moves more than one tile's space during a frame
                _stepPercentageCompleted--;
                finalDestination = TileIncrement();
                //Time.timeScale = 0f;
                if (GetNextTileInteractable())
                {
                    InteractableRouteComplete();
                }
                Vector3 nextTile = GetNextTilePos();
                finalDestination = Vector3.Lerp(finalDestination, nextTile, _stepPercentageCompleted);
            }
            while (_stepPercentageCompleted > 1f);
        }

        transform.position = new Vector3(finalDestination.x, 0f, finalDestination.z);
    }

    private void Step()
    {
        //todo: if stepDistance > 1, check which tile it is
        //Debug.Log(GetNextTilePos());
        Vector3 newTargetPosition = GetNextTilePos();
        if (newTargetPosition.x == Mathf.NegativeInfinity)
        {
            Debug.LogError("Position not set yet?");
            return;
        }
        Vector3 targetPos = new Vector3(newTargetPosition.x, transform.position.y,
            newTargetPosition.z);

        System.Action<ITween<Vector3>> updateNextTile = (t) =>
        {
            Vector3 nextTile = TileIncrement();
            _jumping = false;
            if (GetNextTileInteractable())
            {
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
        return Vector3.Distance(GetCurrentTilePos(),
         GetNextTilePos());
    }

    public void InteractableRouteComplete() // Sent here since it has access to speed for dynamic scaling of rotations
    {
        _incrementedTiming = 0f;
        _interactableRouteTimings.RemoveFirst();
        RouteData newRouteData = _interactableRouteTimings.First();
        //Debug.Log(newRouteData.fullTiming);
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
        float orbChoiceOffset = UnityEngine.Random.Range(0, 1);
        if (orbChoiceOffset == 1)
        {
            orbChoiceOffset = 180f;
        }
        float extraCircles = (Mathf.Max(Mathf.RoundToInt(_fullRouteTiming / _rotationScalar), 1) * 360f) + 180f * Mathf.Sign(angleDiff);
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
        RouteData newRouteData = _interactableRouteTimings.First();
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
        float orbChoiceOffset = UnityEngine.Random.Range(0, 1);
        if (orbChoiceOffset == 1)
        {
            orbChoiceOffset = 180f;
        }
        float extraCircles = (Mathf.Max(Mathf.RoundToInt(_fullRouteTiming / _rotationScalar), 1) * 360f) + orbChoiceOffset * Mathf.Sign(angleDiff);
        _totalDegreesThisRoute = angleDiff + extraCircles;
        //Debug.Log(angleDiff);
        _currentRouteRotation = angleDiff + _startRouteRotation;
        //Debug.Log("New Angle: " + _currentRouteRotation + " >>>>> between interactable " + newRouteData.interactableTilePosition + " and previous " + newRouteData.lastTilePosition);
        orbRotater.SetRadius(Vector3.Distance(newRouteData.interactableTilePosition, newRouteData.lastTilePosition));
        //Time.timeScale = 0f;
    }

    #region Lists and List Methods


     public Vector3 GetNormalizedDirection()
    {
        return (_playerPathPositions[_currentlyTravelledIndex + 1].position
         - _playerPathPositions[_currentlyTravelledIndex].position).normalized;
    }
    // public void SetCurrentTilePosition(Vector3 newPos)
    // {
    //     CurrentTilePosition = newPos;
    //     // if (_playerPathPositions.Count != 0) return;
    //     // _playerPathPositions.RemoveFirst();
    // }

    [NonSerialized]
    public List<PathIndexData> _playerPathPositions = new List<PathIndexData>();

    public struct PathIndexData
    {
        public Vector3 position;
        public bool hasInteractable;

        public PathIndexData(Vector3 newPosition, bool isInteractable)
        {
            this.position = newPosition;
            this.hasInteractable = isInteractable;
        }
    }

    public List<PathIndexData> _pathPositionsToAdd = new List<PathIndexData>();

    public void AddtoPathQueue(PathIndexData newPathData)
    {
        _pathPositionsToAdd.Add(newPathData);
    }

    // Removal will be done on this script with every tile increment!!!!!!!!!!!!

    // public List<PathIndexData> _pathPositionsToRemove = new List<PathIndexData>();

    // public void RemoveFromPathQueue(PathIndexData newPathData)
    // {
    //     _pathPositionsToRemove.Add(newPathData);
    // }

    [NonSerialized]
    public LinkedList<RouteData> _interactableRouteTimings = new LinkedList<RouteData>();

    public struct RouteData
    {
        public float fullTiming;
        public Vector3 lastTilePosition; // Convert the angle between the last two tile into a rotation
        public Vector3 interactableTilePosition;

        public RouteData(float newTiming, Vector3 newLastPos, Vector3 newIntPos)
        {
            this.fullTiming = newTiming;
            this.lastTilePosition = newLastPos;
            this.interactableTilePosition = newIntPos;
        }
    }

    //private int _pathPositionsToRemove = 0;

    public Vector3 TileIncrement()
    {
        //_pathPositionsToRemove++;
        //_playerPathPositions.RemoveAt(0);
        _currentlyTravelledIndex++;
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    // public Vector3 TileDecrement()
    // {
    //     Debug.Log("Increment currentlyTravelled: " + _currentlyTravelledIndex);
    //     return _playerPathPositions[_currentlyTravelledIndex].position;
    // }

    public int _currentlyTravelledIndex = 0;

    public Vector3 GetCurrentTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    public bool GetNextTileInteractable()
    {
        return _playerPathPositions[_currentlyTravelledIndex].hasInteractable;
    }

    public Vector3 GetNextTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex + 1].position;
    }
    #endregion
    
}
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
    public GameObject teleportFx;
    
    [SerializeField]
    private GameEvent _OnPlayerFinishTileEvent;

    [Header("Settings")]
    [SerializeField]
    private KeyCode _movementKey0 = KeyCode.A;
    [SerializeField]
    private KeyCode _movementKey1 = KeyCode.D;
    [SerializeField]
    [Range(0f, 10f)]
    private float _baseSpeed = 1f;
    private float _boostSpeed = 0f;
    [SerializeField]
    private float _boostMaximum = 20f;
    [SerializeField]
    private AnimationCurve _frictionCurve = new AnimationCurve();
    [SerializeField]
    private AnimationCurve _rotationMapping = new AnimationCurve();
    [SerializeField]
    private AnimationCurve _orbGrowth = new AnimationCurve();

    private float _stepPercentageCompleted = 0f;

    [Range(0f, 5f)]
    public float movementTweenSpeed = 0.15f;
    private float _fullSpeed = 0f;
    private bool ready;

    private bool _jumping = false;

    private float _currentRouteBaseTiming = 0f;
    private float _incrementedTiming = 0f;
    private float _startRouteRotation = 0f;

    private float _totalDegreesThisRoute = 0f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1f;
            ready = !ready;
        }
        if (!ready) return;
        if (Input.GetKeyDown(_movementKey0) || Input.GetKeyDown(_movementKey1))
        {
            int keyNum = 0;
            if (Input.GetKeyDown(_movementKey1))
            {
                keyNum = 1;
            }

            orbRotater.CheckOrbAccuracy(keyNum, _interactableRouteTimings);
        }


        Friction();
        _fullSpeed = _baseSpeed + _boostSpeed;
        float distToNext = DistanceToNextTile();
        _incrementedTiming = ((_fullSpeed * Time.deltaTime / _currentRouteBaseTiming) * _totalDegreesThisRoute);
        //_currentRouteBaseTiming
        //_currentRouteRotation

        // Must be additive
        orbRotater.SetOrbSize(_orbGrowth.Evaluate(_boostSpeed));
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
    #region Movement Effects
    public void Slow(float slowAmount)
    {
        _boostSpeed = Mathf.Max(_boostSpeed - Mathf.Max(slowAmount, _fullSpeed * 0.18f), 0f);
    }

    public void Friction()
    {
        _boostSpeed = Mathf.Max(_boostSpeed - (_boostSpeed * _frictionCurve.Evaluate(_boostSpeed) * Time.deltaTime), 0f);
    }

    public void Boost(float boostAmount)
    {
        _boostSpeed = Mathf.Min(_boostSpeed + boostAmount, _boostMaximum);
    }

    public void Stun(float stunDuration)
    {
        StartCoroutine(StunSequence(stunDuration));
    }

    private IEnumerator StunSequence(float seconds)
    {
        //ready = false;
        yield return new WaitForSeconds(seconds);
        Time.timeScale = 1f;
        //ready = true;
    }

    public void SlowMo(int numPresses)
    {
        orbRotater.SlowMoRecovery(numPresses);
    }
    #endregion

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
        
        StartCoroutine(Teleport(targetPos));
    }

    IEnumerator Teleport(Vector3 destination)
    {
        _jumping = true;
        
        yield return new WaitForSeconds(movementTweenSpeed / _fullSpeed);
        
        Vector3 nextTile = TileIncrement();
        _jumping = false;
        if (GetNextTileInteractable())
        {
            InteractableRouteComplete();
        }
        
        GameObject lol = Instantiate(teleportFx, transform.position, Quaternion.identity);
        Destroy(lol, 2f);
        
        transform.position = destination;
        
        GameObject lol1 = Instantiate(teleportFx, transform.position, Quaternion.identity);
        Destroy(lol1, 2f);
    }

    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        return Vector3.Distance(GetCurrentTilePos(),
         GetNextTilePos());
    }

    public void InteractableRouteComplete() // Sent here since it has access to speed for dynamic scaling of rotations
    {
        if (_interactableRouteTimings[0].interactableTile.isActiveAndEnabled)
        {
            _interactableRouteTimings[0].interactableTile.WalkedOver();
        }
        _incrementedTiming = 0f;
        _interactableRouteTimings.RemoveAt(0);
        RouteData newRouteData = _interactableRouteTimings.First();
        _currentRouteBaseTiming = newRouteData.fullTiming;
        Vector3 normalizedDir = (newRouteData.newTilePosition - newRouteData.lastTilePosition).normalized;
        float newAngle = Mathf.Atan2(normalizedDir.x, normalizedDir.z) * Mathf.Rad2Deg;
        if (normalizedDir.z < 0) newAngle = 180f - newAngle;
        else if (normalizedDir.x < 0) newAngle = 360f + newAngle;
        _startRouteRotation = orbRotater._currentAngle;
        float angleDiff = Mathf.DeltaAngle(_startRouteRotation, newAngle);
        _totalDegreesThisRoute = GetTotalRouteDegrees(angleDiff);
        orbRotater.SetRadius(Vector3.Distance(newRouteData.newTilePosition, newRouteData.lastTilePosition));
    }

    public void StartFirstRoute()
    {
        _incrementedTiming = 0f;
        _fullSpeed = _baseSpeed + _boostSpeed;
        RouteData newRouteData = _interactableRouteTimings[0];
        _currentRouteBaseTiming = newRouteData.fullTiming;
        Vector3 normalizedDir = (newRouteData.newTilePosition - newRouteData.lastTilePosition).normalized;
        float newAngle = Mathf.Atan2(normalizedDir.x, normalizedDir.z) * Mathf.Rad2Deg;
        if (normalizedDir.z < 0) newAngle = 180f - newAngle;
        else if (normalizedDir.x < 0) newAngle = 360f + newAngle;
        orbRotater._currentAngle = 90f;
        _startRouteRotation = orbRotater._currentAngle;
        float angleDiff = Mathf.DeltaAngle(_startRouteRotation, newAngle);
        _totalDegreesThisRoute = GetTotalRouteDegrees(angleDiff);
        orbRotater.SetRadius(Vector3.Distance(newRouteData.newTilePosition, newRouteData.lastTilePosition));
    }

    private float GetTotalRouteDegrees(float angleDiff)
    {
        float direction = Mathf.Sign(angleDiff);
        if (direction == -1f || angleDiff == 0f) // 180f is a half rotation
        {
            angleDiff += 180f;
        }

        return angleDiff;
    }

    private float GetRouteRotations(float fullRouteTiming)
    {
        float timingInAdjustedSeconds = fullRouteTiming / _fullSpeed;
        float orbChoiceOffset = UnityEngine.Random.Range(0, 2);
        if (orbChoiceOffset == 1)
        {
            return Mathf.Max((float)Mathf.FloorToInt(timingInAdjustedSeconds * _rotationMapping.Evaluate(_fullSpeed)) - 0.5f);
        }
        return Mathf.Max((float)Mathf.FloorToInt(timingInAdjustedSeconds * _rotationMapping.Evaluate(_fullSpeed)), 1f);
    }

    #region Lists and List Methods


    public Vector3 GetNormalizedDirection()
    {
        return (_playerPathPositions[_currentlyTravelledIndex + 1].position
         - _playerPathPositions[_currentlyTravelledIndex].position).normalized;
    }

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

    [NonSerialized]
    public List<RouteData> _interactableRouteTimings = new List<RouteData>();

    public struct RouteData
    {
        public float fullTiming;
        public Vector3 lastTilePosition; // Convert the angle between the last two tile into a rotation
        public Vector3 newTilePosition;
        public InteractableTile interactableTile;

        public RouteData(float newTiming, Vector3 newLastPos, Vector3 newTilePos, InteractableTile newInteractable)
        {
            this.fullTiming = newTiming;
            this.lastTilePosition = newLastPos;
            this.newTilePosition = newTilePos;
            this.interactableTile = newInteractable;
        }
    }

    public Vector3 TileIncrement()
    {
        _currentlyTravelledIndex++;
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    public int _currentlyTravelledIndex = 0;

    public Vector3 GetCurrentTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    public bool GetNextTileInteractable()
    {
        return _playerPathPositions[_currentlyTravelledIndex].hasInteractable;
    }

    //public bool 

    public Vector3 GetNextTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex + 1].position;
    }
    #endregion
    
}
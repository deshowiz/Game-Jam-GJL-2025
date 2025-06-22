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
    private GameEvent _OnPlayerFinishBiome;

    [Header("Settings")]
    [SerializeField]
    private KeyCode _movementKey0 = KeyCode.A;
    [SerializeField]
    private KeyCode _movementKey1 = KeyCode.D;
    [SerializeField]
    [Range(0f, 10f)]
    private float _baseSpeed = 1f;
    [SerializeField]
    private float _boostSpeed = 0f;
    public float BoostSpeed { get { return _boostSpeed; } }

    [SerializeField]
    private float _boostMaximum = 20f;
    [SerializeField]
    private Vector3 _playerEffectOffset = Vector3.zero;
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

    Vector3 _windowSpriteRatio = Vector3.zero;

    private float _currentPairDistance = 0f;

    private float _currentRouteProgressStep = 0f;
    private float _totalHeightProgress = 0f;
    private bool _isBlueOrb = true;

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
        _currentRouteProgressStep = (_fullSpeed * Time.deltaTime / _currentRouteBaseTiming);
        _totalHeightProgress += _currentRouteProgressStep;
        _incrementedTiming = _currentRouteProgressStep * _totalDegreesThisRoute;
        //_currentRouteBaseTiming
        //_currentRouteRotation

        // Must be additive
        orbRotater.SetOrbSize(_orbGrowth.Evaluate(_boostSpeed));
        orbRotater.SetNewRotation(_incrementedTiming);
        if (_interactableRouteTimings.Count != 0)
        {
            orbRotater.SetOrbHeight(
            Mathf.Lerp(GameManager.Instance.Player.transform.position.y,
            _interactableRouteTimings[0].interactableTile.transform.position.y,
            _totalHeightProgress), _isBlueOrb);
        }

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
        ready = false;
        yield return new WaitForSeconds(seconds);
        Time.timeScale = 1f;
        ready = true;
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
        _currentPairDistance = Vector3.Distance(GetNextTilePos(), GetCurrentTilePos());

        if (_stepPercentageCompleted < _currentPairDistance)
        {
            finalDestination += GetNormalizedDirection() * distanceThisFrame;
        }
        else
        {
            do
            { // In case the player moves more than one tile's space during a frame
                _stepPercentageCompleted -= _currentPairDistance;
                finalDestination = TileIncrement();
                //Time.timeScale = 0f;
                if (GetNextTileInteractable())
                {
                    InteractableRouteComplete();
                }
                Vector3 nextTile = GetNextTilePos();
                if (nextTile.x == Mathf.NegativeInfinity)
                {
                    ready = false;
                    return;
                }
                finalDestination = Vector3.Lerp(finalDestination, nextTile, _stepPercentageCompleted);
                _currentPairDistance = Vector3.Distance(nextTile, GetCurrentTilePos());
            }
            while (_stepPercentageCompleted > _currentPairDistance);
        }

        transform.position = new Vector3(finalDestination.x, finalDestination.y, finalDestination.z);
    }

    private void Step()
    {
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

        // GameObject lol = Instantiate(teleportFx, transform.position + _playerEffectOffset, Quaternion.identity);
        // //lol.transform.localScale = new Vector3(1f, 1.7778f, 1);
        // Destroy(lol, 2f);

        transform.position = destination;

        GameObject lol1 = Instantiate(teleportFx, transform.position + _playerEffectOffset, Quaternion.identity);
        //lol1.transform.localScale = new Vector3(1f, 1.7778f, 1);
        Destroy(lol1, 2f);
        
        AudioManager.Instance.PlaySFX("WAV_GJLSpringJam2025_INT_TeleportV3");
    }

    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePos = GetCurrentTilePos();
        Vector3 nextTilePos = GetNextTilePos();
        return Vector2.Distance(new Vector2(currentTilePos.x, currentTilePos.z),
         new Vector2(nextTilePos.x, nextTilePos.z));
    }

    public void InteractableRouteComplete() // Sent here since it has access to speed for dynamic scaling of rotations
    {
        _totalHeightProgress = 0f;
        if (_interactableRouteTimings[0].interactableTile.isActiveAndEnabled)
        {
            _interactableRouteTimings[0].interactableTile.WalkedOver();
        }
        _incrementedTiming = 0f;
        _interactableRouteTimings.RemoveAt(0);
        if (_interactableRouteTimings.Count == 0)
        {
            orbRotater.EndBiomeSpin();
             return;
        }
        RouteData newRouteData = _interactableRouteTimings.First();
        _currentRouteBaseTiming = newRouteData.fullTiming;
        Vector3 normalizedDir = (newRouteData.newTilePosition - newRouteData.lastTilePosition).normalized;
        float newAngle = Mathf.Atan2(normalizedDir.x, normalizedDir.z) * Mathf.Rad2Deg;
        // if (normalizedDir.z < 0) newAngle = 180f - newAngle;
        // else if (normalizedDir.x < 0) newAngle = 360f + newAngle;
        _startRouteRotation = orbRotater._currentAngle;
        float angleDiff = Mathf.DeltaAngle(_startRouteRotation, newAngle);
        //Debug.Log(IsBlue(angleDiff));
        _isBlueOrb = IsBlue(angleDiff);
        _totalDegreesThisRoute = GetTotalRouteDegrees(angleDiff);
        Vector3 newTilePos = newRouteData.newTilePosition;
        Vector3 lastTilePos = newRouteData.lastTilePosition;
        orbRotater.SetRadius(Vector2.Distance(new Vector2(newTilePos.x, newTilePos.z), new Vector2(lastTilePos.x, lastTilePos.z)));
    }

    public void StartFirstRoute()
    {
        _totalHeightProgress = 0f;
        _incrementedTiming = 0f;
        _fullSpeed = _baseSpeed + _boostSpeed;
        RouteData newRouteData = _interactableRouteTimings[0];
        _currentRouteBaseTiming = newRouteData.fullTiming;
        Vector3 normalizedDir = (newRouteData.newTilePosition - newRouteData.lastTilePosition).normalized;
        float newAngle = Mathf.Atan2(normalizedDir.x, normalizedDir.z) * Mathf.Rad2Deg; ;
        // if (normalizedDir.z < 0) newAngle = 180f - newAngle;
        // else if (normalizedDir.x < 0) newAngle = 360f + newAngle;

        orbRotater._currentAngle = 90f;
        _startRouteRotation = orbRotater._currentAngle;
        float angleDiff = Mathf.DeltaAngle(_startRouteRotation, newAngle);
        //Debug.Log(IsBlue(angleDiff));
        _isBlueOrb = IsBlue(angleDiff);
        _totalDegreesThisRoute = GetTotalRouteDegrees(angleDiff) * 1.15f;
        Vector3 newTilePos = newRouteData.newTilePosition;
        Vector3 lastTilePos = newRouteData.lastTilePosition;
        orbRotater.SetRadius(Vector2.Distance(new Vector2(newTilePos.x, newTilePos.z), new Vector2(lastTilePos.x, lastTilePos.z)));
    }

    private float GetTotalRouteDegrees(float angleDiff)
    {
        float direction = Mathf.Sign(angleDiff);
        if (direction == -1f || angleDiff == 0f) // 180f is a half rotation
        {
            angleDiff += 180f;
            return angleDiff;
        }
        return angleDiff;
    }

    private bool IsBlue(float angleDiff)
    {
        return Mathf.Sign(angleDiff) != -1f;
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
        if (_playerPathPositions == null || _playerPathPositions.Count == 0)
            return Vector3.zero;
   
        if (_currentlyTravelledIndex < 0 || _currentlyTravelledIndex >= _playerPathPositions.Count - 1)
            return Vector3.zero;
   
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
        if (_interactableRouteTimings.Count == 0)
        {
            // ready = false;
            return false;
        }
        return _playerPathPositions[_currentlyTravelledIndex].hasInteractable;
    }

    //public bool 

    public Vector3 GetNextTilePos()
    {
        if (_currentlyTravelledIndex == _playerPathPositions.Count - 1)
        {
            // End Biome Event
            ready = false;
            _OnPlayerFinishBiome.Raise();
            return Vector3.negativeInfinity;
        }
        return _playerPathPositions[_currentlyTravelledIndex + 1].position;
    }

    public void RemoveLastInteractable()
    {
        InteractableTile interactable = _interactableRouteTimings.Last().interactableTile;
        interactable.GetComponent<MeshRenderer>().enabled = false;
        interactable.enabled = false;
    }
    #endregion

}
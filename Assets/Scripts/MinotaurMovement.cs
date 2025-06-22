using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MinotaurMovement : MonoBehaviour
{
    public GameObject teleportFx;

    [Header("References")]
    [SerializeField]
    private Transform _minotaurTransform = null;
    [SerializeField]
    private GameEvent _contactEvent = null;
    [Header("Settings")]
    [SerializeField]
    private float _baseSpeed = 3f;
    [SerializeField]
    private float _boostSpeed = 0f;

    private float _fullSpeed = 0f;
    [Range(0f, 5f)]
    public float movementTweenSpeed = 0.15f;
    [SerializeField]
    private Vector3 _playerEffectOffset = Vector3.zero;
    [SerializeField]
    private float _distanceForContact = 1f;
    [SerializeField]
    private AnimationCurve _rubberBandSpeed = new AnimationCurve();
    [SerializeField]
    private AnimationCurve _levelSpeedProgression = new AnimationCurve();
    [SerializeField]
    private float _playerDistanceCurveWidth = 50f;
    [SerializeField]
    private float _totalTravelCurveWidth = 500f;

    private float speedDistanceAdjuster = 1f;

    private List<Vector3> _tilesToTravel = new List<Vector3>();
    public void AddTileToTravel(Vector3 newTilePosition)
    {
        _tilesToTravel.Add(newTilePosition);
    }
    private float _stepPercentageCompleted = 0f;
    private float _currentPairDistance = 0f;
    private bool ready = false;

    private bool _jumping = false;

    private float _distanceTravelled = 0f;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ready = !ready;
        }
        if (!ready || _tilesToTravel.Count == 0) return;

        if (IsContactWithPlayer())
        {
            Debug.Log("Contact");
            //Time.timeScale = 0f;
            _contactEvent.Raise();
        }
        float curveAdjuster =
        _fullSpeed = (_baseSpeed * _levelSpeedProgression.Evaluate(_distanceTravelled / _totalTravelCurveWidth)) + (speedDistanceAdjuster);
        float distToNext = DistanceToNextTile();
        if (distToNext == 1f) // Distance of greater than 1 indicates a gap since all tiles need to have a diameter of 1 or lower
        {
            Slide();
        }
        else if (!_jumping)/*if (Time.time >= lastStepTime + timeBetweenSteps)*/
        {
            Step();
        }

        if (GameManager.Instance.UIMinotaurBar)
        {
            GameManager.Instance.UIMinotaurBar.UpdateBar(DistanceToPlayer());
        }
        else
        {
            GameManager.Instance.PleaseMinotaurBar();
        }
            
        //Debug.Log("Distance to Player: " + DistanceToPlayer());
        //Debug.Log("Distance to Player: " + LinearDistanceToPlayer());
        //Debug.Log(GetNormalizedDirection());
    }

    public float DistanceToPlayer()
    {
        if (GameManager.Instance.Player == null) GameManager.Instance.Player = GameObject.Find("Player").GetComponent<Player>();
        return GetPathDistance(transform.position, GameManager.Instance.Player.transform.position);
    }

    public float LinearDistanceToPlayer()
    {
        if (GameManager.Instance.Player == null) GameManager.Instance.Player = GameObject.Find("Player").GetComponent<Player>();
        return Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);
    }
    
    public float GetPathDistance(Vector3 minotaurPosition, Vector3 playerPosition)
    {
        if (_tilesToTravel.Count == 0)
            return 0f;
            
        int minotaurClosestIndex = GetClosestPathIndex(minotaurPosition);
        int playerClosestIndex = GetClosestPathIndex(playerPosition);

        int startIndex = Mathf.Min(minotaurClosestIndex, playerClosestIndex);
        int endIndex = Mathf.Max(minotaurClosestIndex, playerClosestIndex);
        
        float totalDistance = 0f;
        
        for (int i = startIndex; i < endIndex; i++)
        {
            totalDistance += Vector3.Distance(_tilesToTravel[i], _tilesToTravel[i + 1]);
        }
        
        return totalDistance;
    }

    private int GetClosestPathIndex(Vector3 position)
    {
        if (_tilesToTravel.Count == 0)
            return -1;
            
        int closestIndex = 0;
        float shortestDistance = Vector3.Distance(_tilesToTravel[0], position);
        
        for (int i = 1; i < _tilesToTravel.Count; i++)
        {
            float distance = Vector3.Distance(_tilesToTravel[i], position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }
    
    public Vector3 GetNormalizedDirection()
    {
        if (_tilesToTravel == null || _tilesToTravel.Count == 0)
        {
            return Vector3.zero; 
        }
    
        int minotaurClosestIndex = GetClosestPathIndex(transform.position);
        
        if (minotaurClosestIndex < 0 || minotaurClosestIndex >= _tilesToTravel.Count - 1)
        {
            return Vector3.zero; 
        }
    
        Vector3 dir1 = _tilesToTravel[minotaurClosestIndex];
        Vector3 dir2 = _tilesToTravel[minotaurClosestIndex + 1];
    
        return (dir1 - dir2).normalized;
    }

    private void Slide()
    {
        Vector3 finalDestination = transform.position;
        float distanceThisFrame = _fullSpeed * Time.deltaTime;
        _stepPercentageCompleted += distanceThisFrame;
        _distanceTravelled += distanceThisFrame;
        _currentPairDistance = Vector3.Distance(_tilesToTravel[1], _tilesToTravel[0]);

        if (_stepPercentageCompleted < _currentPairDistance)
        {
            finalDestination +=
            Vector3.Normalize(_tilesToTravel[1] - _tilesToTravel[0]) * distanceThisFrame;
        }
        else
        {
            do
            { // In case the player moves more than one tile's space during a frame
                _stepPercentageCompleted -= _currentPairDistance;
                finalDestination = TileIncrement();
                //Time.timeScale = 0f;
                Vector3 nextTile = _tilesToTravel[1];
                if (nextTile.x == Mathf.NegativeInfinity)
                {
                    ready = false;
                    return;
                }
                finalDestination = Vector3.Lerp(finalDestination, nextTile, _stepPercentageCompleted);
                _currentPairDistance = Vector3.Distance(nextTile, _tilesToTravel[0]);
            }
            while (_stepPercentageCompleted > _currentPairDistance);
        }

        transform.position = new Vector3(finalDestination.x, finalDestination.y, finalDestination.z);
    }

    private void Step()
    {
        Vector3 newTargetPosition = _tilesToTravel[1];
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
        _distanceTravelled += Vector3.Distance(transform.position, destination);
        transform.position = destination;

        GameObject lol1 = Instantiate(teleportFx, transform.position + _playerEffectOffset, Quaternion.identity);
        //lol1.transform.localScale = new Vector3(1f, 1.7778f, 1);
        Destroy(lol1, 2f);
        
    }

    private Vector3 TileIncrement()
    {
        _tilesToTravel.RemoveAt(0);
        return _tilesToTravel[0];
    }

    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePos = _tilesToTravel[0];
        Vector3 nextTilePos = _tilesToTravel[1];
        return Vector2.Distance(new Vector2(currentTilePos.x, currentTilePos.z),
         new Vector2(nextTilePos.x, nextTilePos.z));
    }

    private bool IsContactWithPlayer()
    {
        //float playerDistanceAway =  Vector3.Distance(GameManager.Instance.Player.transform.position, _minotaurTransform.position);
        float playerDistanceAway =  DistanceToPlayer();
        if (playerDistanceAway <= _distanceForContact) return true;
        speedDistanceAdjuster = _rubberBandSpeed.Evaluate(playerDistanceAway / _playerDistanceCurveWidth);

        return false;
    }
}

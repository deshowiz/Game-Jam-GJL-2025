using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TileGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerMovement _playerMovement = null;
    [SerializeField]
    private Wall _baseWallPrefab = null;
    [SerializeField]
    private List<BiomeInteractableData> _interactableBiomes = null;
    [SerializeField]
    private Transform _tilesParentTransform = null;
    [SerializeField]
    private Tile _baseTilePrefab = null;
    // [SerializeField]
    // private List<Wall> _wallPrefabs = new List<Wall>();
    [SerializeField]
    private List<TileGroup> _tileGroups = new List<TileGroup>();

    [SerializeField]
    private GameEvent _runningEvent = null;

    private Queue<Tile> _currentlyAvailableTiles = new Queue<Tile>();
    private Queue<Wall> _currentlyAvailableWalls = new Queue<Wall>();
    private List<Tile> _steppingTileQueue = new List<Tile>();
    private LinkedList<Wall> _usedWallTileQueue = new LinkedList<Wall>();

    [Header("Settings")]
    [SerializeField]
    private int _maximumQueueSpawnSize = 50;
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Percentage of spawned interactables that are boosts instead of traps")]
    private float _boostPercentage = 0.5f;

    [Tooltip("Set Initial Height of tiles with this variable")]
    [SerializeField]
    private Vector3 _lastPosition = Vector3.zero;
    [SerializeField]
    private int maxPositiveInteractableScore = 2;
    [SerializeField]
    private int maxNegativeInteractableScore = -3;
    private int currentInteractableScore = 0;
    private int _repeatPositiveInteractables = 0;

    private bool _isRunning = false;

    private List<TileGroup.PositionedGroup> _currentTileGroup = null;

    private int _tileGroupStepIndex = 0;
    private Vector3 _groupAnchorPosition = Vector3.negativeInfinity;
    private uint _lastInteractableTileIndex = 0;
    private uint _fullTileIndexCount = 0;

    private BiomeInteractableData _currentBiome = null;

    private bool _forcedBoost = true;

    private float _interactableRouteTimingTotal = 0f;

    private float _lastTileTiming = 0f;

    public void FullInitialization()
    {
        if (_playerMovement == null)
        {
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        InitializeTiles();
    }

    private void SetCurrentBiome(int biomeIndex)
    {
        _currentBiome = _interactableBiomes[biomeIndex];
        _currentBiome.InitializeBiome();
    }

    private void InitializeTiles() // Change to add integer parameter for reloading scene as next biome
    {
        SetCurrentBiome(0); // Swap biome specific texture in beginning of scene for base prefab before cloning
        // OR material in case color over settings need changing
        _lastInteractableTileIndex = 5;
        for (int i = 0; i < _maximumQueueSpawnSize; i++)
        {
            Tile tileCopy = Instantiate(_baseTilePrefab, _tilesParentTransform);
            _currentlyAvailableTiles.Enqueue(tileCopy);
        }

        for (int i = 0; i < _maximumQueueSpawnSize; i++)
        {
            Wall wallCopy = Instantiate(_baseWallPrefab, _tilesParentTransform);
            _currentlyAvailableWalls.Enqueue(wallCopy);
        }

        SetNewTileGroup();
        for (int i = 0; i < 2; i++)
        {
            PlaceNextTile();
        }
        float firstDistValue = Vector3.Distance(_steppingTileQueue[0].transform.position,
         _steppingTileQueue[1].transform.position);
        if (firstDistValue > 1f)
        {
            _interactableRouteTimingTotal -= firstDistValue;
        }
        else
        {
            _interactableRouteTimingTotal -= 1f;
        }
        
        //Debug.Log(_lastTileTiming);
        for (int i = 2; i < _maximumQueueSpawnSize / 2; i++)
        {
            PlaceNextTile();
        }
        _lastPosition = _steppingTileQueue[0].transform.position;
        GameManager.Instance._nextTilePosition = _lastPosition;
        _isRunning = true;
        //Debug.Log("Raising running event");
        _runningEvent.Raise();
    }

    public void Reset()
    {
        //_currentTileIndex = 0;
        // if (_usedTileQueue != null)
        // {
        //     for (int i = _usedTileQueue.Count; i > 0; i--)
        //     {
        //         RemoveTile();
        //     }
        //     for (int i = _steppingTileQueue.Count; i > 0; i--)
        //     {
        //         Tile lastRemovedTile = _steppingTileQueue.First();
        //         _steppingTileQueue.RemoveFirst();
        //         lastRemovedTile.gameObject.SetActive(false);
        //         _currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
        //     }
        //     //Debug.Log(_usedTileQueue.Count);
        // }
    }

    private void Update()
    {
        if (_isRunning)
        {
            if (_steppingTileQueue.Count < _maximumQueueSpawnSize / 2)
            {
                PlaceNextTile();
            }
            if (_steppingTileQueue.Count != 0 && Vector3.Distance(GameManager.Instance.Player.transform.position, _steppingTileQueue[0].transform.position) > 20f) // Magic number to change when tweaking later with variable
            {
                RemoveTile();
                //Debug.Log(GameManager.Instance.Player.position.x + " > " + _usedWallTileQueue.Last().transform.position.x);
                if (_usedWallTileQueue.Count != 0 && Vector3.Distance(GameManager.Instance.Player.transform.position, _usedWallTileQueue.First().transform.position) > 20f
                && GameManager.Instance.Player.transform.position.x > _usedWallTileQueue.First().transform.position.x) // Magic number to change when tweaking later with variable
                {
                    RemoveWall();
                }
            }
            _currentBiome.UpdateInteractables();

        }
    }

    private void RemoveTile()
    {
        Tile lastRemovedTile = _steppingTileQueue.First();
        _steppingTileQueue.RemoveAt(0);

        lastRemovedTile.gameObject.SetActive(false);
        _currentlyAvailableTiles.Enqueue(lastRemovedTile);
    }

    private void RemoveWall()
    {
        Wall lastRemovedWall = _usedWallTileQueue.First();
        _usedWallTileQueue.RemoveFirst();
        lastRemovedWall.gameObject.SetActive(false);
        _currentlyAvailableWalls.Enqueue(lastRemovedWall);
    }

    private void PlaceNextTile()
    {
        Tile newPlaceableTile = _currentlyAvailableTiles.Dequeue();
        newPlaceableTile.transform.position = _groupAnchorPosition + _currentTileGroup[_tileGroupStepIndex].position;
        newPlaceableTile.SetMesh(_currentTileGroup[_tileGroupStepIndex].tileMeshPrefab);
        Vector3 newTilePosition = newPlaceableTile.transform.position;

        newPlaceableTile.gameObject.SetActive(true);
        _steppingTileQueue.Add(newPlaceableTile);
        _tileGroupStepIndex++;

        _fullTileIndexCount++;
        bool isInteractable = _fullTileIndexCount >= _lastInteractableTileIndex;
        _playerMovement.AddtoPathQueue(new PlayerMovement.PathIndexData(newTilePosition, isInteractable));
        Vector3 storedLastPos = _lastPosition;

        if (isInteractable)
        {
            InteractableTile newInteractable = SetNextInteractable(newTilePosition);
            //Debug.Log("Route Timing: " + interactableRouteTimingTotal);
            _playerMovement._interactableRouteTimings.Add(
                new PlayerMovement.RouteData(_interactableRouteTimingTotal, storedLastPos, newTilePosition, newInteractable));
            _interactableRouteTimingTotal = 0f;
        }
        else
        {
            float currentPairDistance = Vector3.Distance(_lastPosition, newTilePosition);
            if (currentPairDistance == 1)
            {
                _lastTileTiming = 1f;
                _interactableRouteTimingTotal += 1f;
            }
            else
            {
                _lastTileTiming = GameManager.Instance._jumpBaseTiming;
                _interactableRouteTimingTotal += _lastTileTiming;
            }
        }
        _lastPosition = newTilePosition;
        if (_tileGroupStepIndex >= _currentTileGroup.Count)
        {
            SetNewTileGroup();
        }
    }

    private InteractableTile SetNextInteractable(Vector3 newPos)
    {
        Vector3 placementPos = newPos + new Vector3(0f, 10f, 0f);
        //Debug.Log("Interactable position: " + placementPos);
        if (_forcedBoost)
        {
            _forcedBoost = false;
            _lastInteractableTileIndex = _lastInteractableTileIndex + (uint)_currentBiome.GetNextBoostPosition();
            currentInteractableScore++;
            return _currentBiome.SetRandomPowerup(placementPos);
        }
        if (currentInteractableScore == maxNegativeInteractableScore)
        {
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextBoostPosition();
            _repeatPositiveInteractables++;
            currentInteractableScore++;
            return _currentBiome.SetRandomPowerup(placementPos);
        }

        if (currentInteractableScore == maxPositiveInteractableScore)
        {
            currentInteractableScore--;
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextTrapPosition();
            _repeatPositiveInteractables = 0;
            return _currentBiome.SetRandomTrap(placementPos);
        }

        if (_repeatPositiveInteractables == 2)
        {
            _repeatPositiveInteractables = 1;
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextTrapPosition();
            return _currentBiome.SetRandomTrap(placementPos);
        }
        float randomNum = UnityEngine.Random.Range(0f, 1f);
        if (randomNum <= _boostPercentage)
        {
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextBoostPosition();
            _repeatPositiveInteractables++;
            return _currentBiome.SetRandomPowerup(placementPos);
        }
        else
        {
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextTrapPosition();
            _repeatPositiveInteractables = 0;
            return _currentBiome.SetRandomTrap(placementPos);
        }
    }

    private void SetNewTileGroup()
    {
        TileGroup newTileGroup = _tileGroups[UnityEngine.Random.Range(0, _tileGroups.Count)];
        _currentTileGroup = newTileGroup.PositionedTilePrefabs;
        List<TileGroup.WallData> wallPositions = newTileGroup.WallSectionVariation(UnityEngine.Random.Range(0, newTileGroup.WallSectionCount));
        _groupAnchorPosition = _lastPosition + Vector3.right;
        for (int i = 0; i < wallPositions.Count; i++)
        {
            Wall newPlaceableWall = _currentlyAvailableWalls.Dequeue();
            newPlaceableWall.transform.position = _groupAnchorPosition + wallPositions[i].position;
            newPlaceableWall.SetMesh(wallPositions[i].wallMeshPrefab);
            _usedWallTileQueue.AddLast(newPlaceableWall);
            newPlaceableWall.gameObject.SetActive(true);
        }

        _tileGroupStepIndex = 0;
    }
}

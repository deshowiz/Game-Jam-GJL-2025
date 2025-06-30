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
    private MinotaurMovement _minotaurMovement = null;
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
    [SerializeField]
    private int currentBiomeIndex = 0;
    private int currentInteractableScore = 0;
    private int _repeatPositiveInteractables = 0;

    private bool _isRunning = false;
    private bool _generateLevelEnd = false;
    private bool _startedLevelEnd = false;
    private bool _finishedBiome = false;

    private int _numGroupsGenerated = 0;
    private int _biomeGroupLimit = 0;

    private List<TileGroup.PositionedGroup> _currentTileGroup = null;

    private int _tileGroupStepIndex = 0;
    private Vector3 _groupAnchorPosition = Vector3.negativeInfinity;
    private int _lastInteractableTileIndex = 0;
    private int _fullTileIndexCount = -1;

    private BiomeInteractableData _currentBiome = null;

    private bool _forcedBoost = true;

    private float _interactableRouteTimingTotal = 0f;

    private float _lastTileTiming = 0f;

    public void FullInitialization(int newBiomeIndex)
    {
        SetCurrentBiome(newBiomeIndex);
        _baseTilePrefab = _currentBiome.BasePrefab;
        if (_playerMovement == null)
        {
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
        if (_minotaurMovement == null)
        {
            _minotaurMovement = FindFirstObjectByType<MinotaurMovement>();
        }
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        InitializeTiles();
    }

    public void SetCurrentBiome(int biomeIndex)
    {
        currentBiomeIndex = biomeIndex;
        _currentBiome = _interactableBiomes[currentBiomeIndex];
        _currentBiome.InitializeBiome();
    }

    private void InitializeTiles() // Change to add integer parameter for reloading scene as next biome
    {
        _tileGroupStepIndex = 0;
        //SetCurrentBiome(currentBiomeIndex); // Swap biome specific texture in beginning of scene for base prefab before cloning
        // OR material in case color over settings need changing
        _biomeGroupLimit = _currentBiome.GroupLevelLength;
        _baseWallPrefab = _currentBiome.BaseWallPrefab;
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

        PlaceRowTiles();

        SetNewTileGroup();
        for (int i = 0; i < 2; i++)
        {
            PlaceNextTile();
        }

        _interactableRouteTimingTotal -= 1f;


        for (int i = 2; i < _maximumQueueSpawnSize / 2; i++)
        {
            PlaceNextTile();
        }
        //_lastPosition = _steppingTileQueue[0].transform.position;
        GameManager.Instance._nextTilePosition = _lastPosition;
        _isRunning = true;

        _runningEvent.Raise();
    }

    private void SpawnMinotarRow()
    {
        
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
            Vector2 playerXZ = new Vector2(GameManager.Instance.Player.transform.position.x, GameManager.Instance.Player.transform.position.z);
            if (!_finishedBiome && _steppingTileQueue.Count < _maximumQueueSpawnSize / 2)
            {
                PlaceNextTile();
                _currentBiome.UpdateInteractables(playerXZ);
            }
            
            Vector3 steppingTile0Pos = _steppingTileQueue[0].transform.position;
            if (_steppingTileQueue.Count != 0)
            {
                if (Vector2.Distance(playerXZ, new Vector2(steppingTile0Pos.x, steppingTile0Pos.z)) > 20f
                && playerXZ.x > steppingTile0Pos.x) // Magic number to change when tweaking later with variable
                {
                    RemoveTile();
                    if (_usedWallTileQueue.Count != 0)
                    {
                        Vector3 usedWallTilePos = _usedWallTileQueue.First().transform.position;
                        if (_usedWallTileQueue.Count != 0 && Vector2.Distance(playerXZ, new Vector2(usedWallTilePos.x, usedWallTilePos.z)) > 20f
                        && playerXZ.x > usedWallTilePos.x) // Magic number to change when tweaking later with variable
                        {
                            RemoveWall();
                        }
                    }
                }
            }
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

    private void PlaceRowTiles()
    {
        TileGroup starterGroup = _currentBiome.StartBiomeGroup;
        List<TileGroup.PositionedGroup> tileData = starterGroup.PositionedTilePrefabs;
        for (int i = 0; i < tileData.Count; i++)
        {
            Tile newPlaceableTile = _currentlyAvailableTiles.Dequeue();
            newPlaceableTile.transform.position = tileData[i].position - new Vector3(0f, 10f, 0f);
            newPlaceableTile.SetMesh(tileData[i].tileMeshPrefab);
            newPlaceableTile.gameObject.SetActive(true);
            _minotaurMovement.AddTileToTravel(newPlaceableTile.transform.position + new Vector3(0f, 10f, 0f)); // Magic Y for offset;
        }
        _minotaurMovement.transform.position = tileData[0].position;
    }

    private void PlaceNextTile()
    {
        Tile newPlaceableTile = _currentlyAvailableTiles.Dequeue();
        // Debug.Log(_currentTileGroup.Count);
        // Debug.Log(_tileGroupStepIndex);
        newPlaceableTile.transform.position = _groupAnchorPosition + _currentTileGroup[_tileGroupStepIndex].position;
        newPlaceableTile.SetMesh(_currentTileGroup[_tileGroupStepIndex].tileMeshPrefab);
        Vector3 newTilePosition = newPlaceableTile.transform.position + new Vector3(0f, 10f, 0f); // Magic Y for offset

        newPlaceableTile.gameObject.SetActive(true);
        _steppingTileQueue.Add(newPlaceableTile);
        _tileGroupStepIndex++;

        _fullTileIndexCount++;
        bool isInteractable = _fullTileIndexCount >= _lastInteractableTileIndex;
        _playerMovement.AddtoPathQueue(new PlayerMovement.PathIndexData(newTilePosition, isInteractable));
        _minotaurMovement.AddTileToTravel(newTilePosition);
        Vector3 storedLastPos = _lastPosition;

        if (isInteractable)
        {
            InteractableTile newInteractable = SetNextInteractable(newTilePosition);
            _playerMovement._interactableRouteTimings.Add(
            new PlayerMovement.RouteData(_interactableRouteTimingTotal, storedLastPos, newTilePosition, newInteractable));
            _interactableRouteTimingTotal = 0f;
        }
        else
        {
            float currentPairDistance = Vector2.Distance(new Vector2(_lastPosition.x, _lastPosition.z),
                 new Vector2(newTilePosition.x, newTilePosition.z));
            if (currentPairDistance == 1)
            {
                float fullDistance = Vector3.Distance(_lastPosition + new Vector3(0f, 10f, 0f), newTilePosition);
                if (fullDistance == 1f)
                {
                    _lastTileTiming = 1f;
                    _interactableRouteTimingTotal += 1f;
                }
                else
                {
                    _lastTileTiming = fullDistance;
                    _interactableRouteTimingTotal += _lastTileTiming;
                }

            }
            else
            {
                _lastTileTiming = GameManager.Instance._jumpBaseTiming;
                _interactableRouteTimingTotal += _lastTileTiming;
            }
        }
        _lastPosition = newTilePosition - new Vector3(0f, 10f, 0f); // Magic Y again
        if (_tileGroupStepIndex >= _currentTileGroup.Count)
        {
            _tileGroupStepIndex = 0;
            SetNewTileGroup();
        }
    }

    private InteractableTile SetNextInteractable(Vector3 newPos)
    {
        Vector3 placementPos = newPos;
        //Debug.Log("Interactable position: " + placementPos);
        if (_forcedBoost)
        {
            _forcedBoost = false;
            _lastInteractableTileIndex = _lastInteractableTileIndex + _currentBiome.GetNextBoostPosition();
            currentInteractableScore++;
            return _currentBiome.SetRandomPowerup(placementPos);
        }
        if (currentInteractableScore == maxNegativeInteractableScore)
        {
            _lastInteractableTileIndex = _fullTileIndexCount + _currentBiome.GetNextBoostPosition();
            _repeatPositiveInteractables++;
            currentInteractableScore++;
            return _currentBiome.SetRandomPowerup(placementPos);
        }

        if (currentInteractableScore == maxPositiveInteractableScore)
        {
            currentInteractableScore--;
            _lastInteractableTileIndex = _fullTileIndexCount + _currentBiome.GetNextTrapPosition();
            _repeatPositiveInteractables = 0;
            return _currentBiome.SetRandomTrap(placementPos);
        }

        if (_repeatPositiveInteractables == 2)
        {
            _repeatPositiveInteractables = 1;
            _lastInteractableTileIndex = _fullTileIndexCount + _currentBiome.GetNextTrapPosition();
            return _currentBiome.SetRandomTrap(placementPos);
        }
        float randomNum = UnityEngine.Random.Range(0f, 1f);
        if (randomNum <= _boostPercentage)
        {
            _lastInteractableTileIndex = _fullTileIndexCount + _currentBiome.GetNextBoostPosition();
            _repeatPositiveInteractables++;
            return _currentBiome.SetRandomPowerup(placementPos);
        }
        else
        {
            _lastInteractableTileIndex = _fullTileIndexCount + _currentBiome.GetNextTrapPosition();
            _repeatPositiveInteractables = 0;
            return _currentBiome.SetRandomTrap(placementPos);
        }
    }

    private void SetNewTileGroup()
    {
        _numGroupsGenerated++;
        if (_numGroupsGenerated >= _biomeGroupLimit)
        {
            _generateLevelEnd = true;
        }
        TileGroup newTileGroup;
        if (_generateLevelEnd)
        {
            if (_startedLevelEnd)
            {
                _finishedBiome = true;
                _playerMovement.RemoveLastInteractable();
                return;
            }
            ;
            newTileGroup = _currentBiome.LastBiomeGroup;
            _startedLevelEnd = true;
        }
        else
        {
            //newTileGroup = _tileGroups[UnityEngine.Random.Range(0, _tileGroups.Count)];
            newTileGroup = _currentBiome.RandomTileGroup;
        }
        _currentTileGroup = newTileGroup.PositionedTilePrefabs;
        List<TileGroup.WallData> wallPositions = newTileGroup.WallSection.wallDataList;
        _groupAnchorPosition = _lastPosition + Vector3.right;
        for (int i = 0; i < wallPositions.Count; i++)
        {
            Wall newPlaceableWall = _currentlyAvailableWalls.Dequeue();
            newPlaceableWall.transform.position = _groupAnchorPosition + wallPositions[i].position;
            newPlaceableWall.SetMesh(wallPositions[i].wallMeshPrefab);
            _usedWallTileQueue.AddLast(newPlaceableWall);
            newPlaceableWall.gameObject.SetActive(true);
        }

        
    }
}

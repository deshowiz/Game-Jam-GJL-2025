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
    private List<BiomeInteractableData> _interactableBiomes = null;
    [SerializeField]
    private Transform _tilesParentTransform = null;
    [SerializeField]
    private List<Tile> _tilePrefabs = new List<Tile>();
    [SerializeField]
    private List<Tile> _wallPrefabs = new List<Tile>();
    [SerializeField]
    private List<TileGroup> _tileGroups = new List<TileGroup>();
    [SerializeField]
    private GameEventListener _steppedEventListener = null;

    [SerializeField]
    private GameEvent _runningEvent = null;
    [SerializeField]
    private GameEvent _newGap = null;

    private List<Queue<Tile>> _currentlyAvailableTiles = new List<Queue<Tile>>();
    private List<Queue<Tile>> _currentlyAvailableWalls = new List<Queue<Tile>>();

    // Create a Queue that waits until a certain number of tiles have been collected before sending them back to available lists
    //private LinkedList<Tile> _usedTileQueue = new LinkedList<Tile>();
    private LinkedList<Tile> _steppingTileQueue = new LinkedList<Tile>();
    private LinkedList<Tile> _usedWallTileQueue = new LinkedList<Tile>();

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

    private bool _isRunning = false;

    private List<TileGroup.PositionedGroup> _currentTileGroup = null;
    private LinkedList<Vector3> _gapList = new LinkedList<Vector3>();

    private int _tileGroupStepIndex = 0;
    private Vector3 _groupAnchorPosition = Vector3.negativeInfinity;
    private uint _lastInteractableTileIndex = 0;
    private uint _fullTileIndexCount = 0;

    private BiomeInteractableData _currentBiome = null;

    private bool _forcedBoost = true;

    public void FullInitialization()
    {
        if (_steppedEventListener.Event == null)
        {
            Debug.LogError("Stepped event not assigned to listener");
        }
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        InitializeTiles();
    }

    private void SetCurrentBiome(int biomeIndex)
    {
        _currentBiome = _interactableBiomes[biomeIndex];
        _currentBiome.InitializeBiome();
    }

    private void InitializeTiles()
    {
        SetCurrentBiome(0);
        _lastInteractableTileIndex = 5;
        int tilePrefabIndex = 0;
        foreach (Tile tilePrefab in _tilePrefabs)
        {
            _currentlyAvailableTiles.Add(new Queue<Tile>());
            for (int i = 0; i < _maximumQueueSpawnSize; i++)
            {
                Tile tileCopy = Instantiate(tilePrefab, _tilesParentTransform);
                tileCopy._listIndex = tilePrefabIndex;
                _currentlyAvailableTiles[tilePrefabIndex].Enqueue(tileCopy);
            }
            tilePrefabIndex++;
        }

        tilePrefabIndex = 0;
        foreach (Tile wallPrefab in _wallPrefabs)
        {
            _currentlyAvailableWalls.Add(new Queue<Tile>());
            for (int i = 0; i < _maximumQueueSpawnSize; i++)
            {
                Tile wallCopy = Instantiate(wallPrefab, _tilesParentTransform);
                wallCopy._listIndex = tilePrefabIndex;
                _currentlyAvailableWalls[tilePrefabIndex].Enqueue(wallCopy);
            }
            tilePrefabIndex++;
        }
        SetNewTileGroup();
        PlaceNextTile();
        //SteppedTile();
        PlaceNextTile();
        _lastPosition = _steppingTileQueue.First().transform.position;
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
            if (_steppingTileQueue.Count != 0 && Vector3.Distance(GameManager.Instance.Player.position, _steppingTileQueue.First().transform.position) > 20f) // Magic number to change when tweaking later with variable
            {
                RemoveTile();
                //Debug.Log(GameManager.Instance.Player.position.x + " > " + _usedWallTileQueue.Last().transform.position.x);
                if (_usedWallTileQueue.Count != 0 && Vector3.Distance(GameManager.Instance.Player.position, _usedWallTileQueue.First().transform.position) > 20f
                && GameManager.Instance.Player.position.x > _usedWallTileQueue.First().transform.position.x) // Magic number to change when tweaking later with variable
                {
                    RemoveWall();
                }
            }
            _currentBiome.UpdateInteractables();

        }
    }

    // public void SteppedTile()
    // {
    //     Tile lastRemovedTile = _steppingTileQueue.First();

    //     GameManager.Instance.SetCurrentTilePosition(lastRemovedTile.transform.position);

    //     _steppingTileQueue.RemoveFirst();
    //     //_usedTileQueue.AddLast(lastRemovedTile);
    //     //lastRemovedTile.gameObject.SetActive(false);
    //     //_currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
    //     if (_steppingTileQueue.Count != 0)
    //     {
    //         GameManager.Instance._nextTilePosition = _steppingTileQueue.First().transform.position;
    //     }
    //     // else
    //     // {
    //     //     Debug.LogError("Stepping with no stepping tiles");
    //     // }
    // }

    private void RemoveTile()
    {
        Tile lastRemovedTile = _steppingTileQueue.First();
        _steppingTileQueue.RemoveFirst();
        lastRemovedTile.gameObject.SetActive(false);
        _currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
        GameManager.Instance._playerPathPositions.RemoveAt(0);
        GameManager.Instance._currentlyTravelledIndex--;
    }

    private void RemoveWall()
    {
        Tile lastRemovedWall = _usedWallTileQueue.First();
        _usedWallTileQueue.RemoveFirst();
        lastRemovedWall.gameObject.SetActive(false);
        _currentlyAvailableWalls[lastRemovedWall._listIndex].Enqueue(lastRemovedWall);
    }

    private void PlaceNextTile()
    {
        int chosenOption = _currentTileGroup[_tileGroupStepIndex].tilePrefab._listIndex;
        Tile newPlaceableTile = _currentlyAvailableTiles[chosenOption].Dequeue();
        newPlaceableTile.transform.position = _groupAnchorPosition + _currentTileGroup[_tileGroupStepIndex].position;
        Vector3 newTilePosition = newPlaceableTile.transform.position;
        GameManager.Instance._playerPathPositions.Add(newTilePosition);
        
        _lastPosition = newTilePosition;
        newPlaceableTile.gameObject.SetActive(true);
        _steppingTileQueue.AddLast(newPlaceableTile);
        _tileGroupStepIndex++;
        if (_tileGroupStepIndex >= _currentTileGroup.Count)
        {
            SetNewTileGroup();
        }
        _fullTileIndexCount++;
        if (_fullTileIndexCount >= _lastInteractableTileIndex)
        {
            SetNextInteractable();
        }
    }

    private void SetNextInteractable()
    {
        if (_forcedBoost)
        {
            _forcedBoost = false;
            _currentBiome.SetRandomPowerup(_lastPosition + new Vector3(0f, 10.01f, 0f)); // Magic number set until we merge and I can make it a setting without disrupting scene
            _lastInteractableTileIndex = _lastInteractableTileIndex + (uint)_currentBiome.GetNextBoostPosition();
            return;
        }
        //Debug.Log("Setting next interactable at index " + _fullTileIndexCount);
        float randomNum = UnityEngine.Random.Range(0f, 1f);
        if (randomNum <= _boostPercentage)
        {
            //Debug.Log(_currentBiome);
            _currentBiome.SetRandomPowerup(_lastPosition + new Vector3(0f, 10.01f, 0f)); // Magic number set until we merge and I can make it a setting without disrupting scene
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextBoostPosition();
        }
        else
        {
            _currentBiome.SetRandomTrap(_lastPosition + new Vector3(0f, 10.01f, 0f)); // ^
            _lastInteractableTileIndex = _fullTileIndexCount + (uint)_currentBiome.GetNextTrapPosition();
        }
    }

    private void SetNewTileGroup()
    {
        TileGroup newTileGroup = _tileGroups[UnityEngine.Random.Range(0, _tileGroups.Count)];
        _currentTileGroup = newTileGroup.PositionedTilePrefabs;
        List<Vector3> wallPositions = newTileGroup.WallSectionVariation(UnityEngine.Random.Range(0, newTileGroup.WallSectionCount));
        int newWallIndex = newTileGroup.WallPrefab._listIndex;
        _groupAnchorPosition = _lastPosition + Vector3.right;
        for (int i = 0; i < wallPositions.Count; i++)
        {
            Tile newPlaceableWall = _currentlyAvailableWalls[newWallIndex].Dequeue();
            newPlaceableWall.transform.position = _groupAnchorPosition + wallPositions[i];
            _usedWallTileQueue.AddLast(newPlaceableWall);
            newPlaceableWall.gameObject.SetActive(true);
        }

        _tileGroupStepIndex = 0;
    }

    public void RemoveStepGap()
    {
        //_gapList.RemoveFirst();
    }
}

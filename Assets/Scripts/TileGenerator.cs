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

    private List<Queue<Tile>> _currentlyAvailableTiles = new List<Queue<Tile>>();
    private List<Queue<Tile>> _currentlyAvailableWalls = new List<Queue<Tile>>();

    // Create a Queue that waits until a certain number of tiles have been collected before sending them back to available lists
    private LinkedList<Tile> _usedTileQueue = new LinkedList<Tile>();
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
    private int _tileGroupStepIndex = 0;
    private Vector3 _groupAnchorPosition = Vector3.negativeInfinity;
    private uint _lastInteractableTileIndex = 0;
    private uint _fullTileIndexCount = 0;

    private BiomeInteractableData _currentBiome = null;

    //private Tile _furthestTile = null;

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
        SteppedTile();
        PlaceNextTile();
        _lastPosition = _steppingTileQueue.First().transform.position;
        GameManager.Instance._nextTilePosition = _lastPosition;
        _isRunning = true;
        Debug.Log("Raising running event");
        _runningEvent.Raise();
    }

    public void Reset()
    {
        //_currentTileIndex = 0;
        if (_usedTileQueue != null)
        {
            for (int i = _usedTileQueue.Count; i > 0; i--)
            {
                RemoveTile();
            }
            for (int i = _steppingTileQueue.Count; i > 0; i--)
            {
                Tile lastRemovedTile = _steppingTileQueue.First();
                _steppingTileQueue.RemoveFirst();
                lastRemovedTile.gameObject.SetActive(false);
                _currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
            }
            Debug.Log(_usedTileQueue.Count);
        }
    }

    private void Update()
    {
        if (_isRunning)
        {
            if (_steppingTileQueue.Count < _maximumQueueSpawnSize / 2)
            {
                PlaceNextTile();
            }
            if (_usedTileQueue.Count != 0 && Vector3.Distance(GameManager.Instance.Player.position, _usedTileQueue.First().transform.position) > 20f) // Magic number to change when tweaking later with variable
            {
                RemoveTile();
                //Debug.Log(GameManager.Instance.Player.position.x + " > " + _usedWallTileQueue.Last().transform.position.x);
                if (_usedWallTileQueue.Count != 0 && Vector3.Distance(GameManager.Instance.Player.position, _usedWallTileQueue.First().transform.position) > 20f
                && GameManager.Instance.Player.position.x > _usedWallTileQueue.First().transform.position.x) // Magic number to change when tweaking later with variable
                {
                    RemoveWall();
                }
            }
            
        }
    }

    public void SteppedTile()
    {
        Tile lastRemovedTile = _steppingTileQueue.First();
        
        GameManager.Instance.CurrentTilePosition = lastRemovedTile.transform.position;
        
        _steppingTileQueue.RemoveFirst();
        _usedTileQueue.AddLast(lastRemovedTile);
        //lastRemovedTile.gameObject.SetActive(false);
        //_currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
        if (_steppingTileQueue.Count != 0)
        {
            GameManager.Instance._nextTilePosition = _steppingTileQueue.First().transform.position;
        }
        // else
        // {
        //     Debug.LogError("Stepping with no stepping tiles");
        // }
    }

    private void RemoveTile()
    {
        Tile lastRemovedTile = _usedTileQueue.First();
        _usedTileQueue.RemoveFirst();
        lastRemovedTile.gameObject.SetActive(false);
        _currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
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
        _lastPosition = newPlaceableTile.transform.position;
        newPlaceableTile.gameObject.SetActive(true);
        _steppingTileQueue.AddLast(newPlaceableTile);
        _tileGroupStepIndex++;
        if (_tileGroupStepIndex >= _currentTileGroup.Count)
        {
            SetNewTileGroup();
        }
        _fullTileIndexCount++;
        if (_fullTileIndexCount == _lastInteractableTileIndex)
        {
            if (UnityEngine.Random.Range(0f, 1f) <= _boostPercentage)
            {
                SetNextBoostInteractable();
            }
            else
            {
                //SetNextTrapInteractable();
            }
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

    private void SetNextBoostInteractable()
    {
        // uint tileIndexAdditive = _currentBiome.GetNextBoost();
        // _lastInteractableTileIndex += tileIndexAdditive;
    }
}

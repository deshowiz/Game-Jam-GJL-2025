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
    private Transform _tilesParentTransform = null;
    [SerializeField]
    private List<Tile> _tilePrefabs = new List<Tile>();
    [SerializeField]
    private List<TileGroup> _tileGroups = new List<TileGroup>();
    [SerializeField]
    private GameEventListener _steppedEventListener = null;

    private List<Queue<Tile>> _currentlyAvailableTiles = new List<Queue<Tile>>();

    // Create a Queue that waits until a certain number of tiles have been collected before sending them back to available lists
    private LinkedList<Tile> _usedTileQueue = new LinkedList<Tile>();
    private LinkedList<Tile> _steppingTileQueue = new LinkedList<Tile>();

    [Header("Settings")]
    [SerializeField]
    private int _maximumQueueSpawnSize = 50;
    [SerializeField]
    private List<Vector3> _positions = new List<Vector3>(); //  To be changed for more complexity later
    //private int _currentTileIndex = 0;

    [Tooltip("Set Initial Height of tiles with this variable")]
    [SerializeField]
    private Vector3 _lastPosition = Vector3.zero;

    private bool _isRunning = false;

    private List<TileGroup.PositionedGroup> _currentTileGroup = null;
    private int _tileGroupStepIndex = 0;
    private Vector3 _groupAnchorPosition = Vector3.negativeInfinity;

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

    private void InitializeTiles()
    {
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
        SetNewTileGroup();
        _tileGroupStepIndex = 0;
        PlaceNextTile();
        SteppedTile();
        PlaceNextTile();
        _lastPosition = _steppingTileQueue.First().transform.position;
        GameManager.Instance._nextTilePosition = _lastPosition;
        _isRunning = true;
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
            }
        }
    }

    public void SteppedTile()
    {
        Tile lastRemovedTile = _steppingTileQueue.First();
        _steppingTileQueue.RemoveFirst();
        _usedTileQueue.AddLast(lastRemovedTile);
        //lastRemovedTile.gameObject.SetActive(false);
        //_currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
        if (_steppingTileQueue.Count != 0)
        {
            GameManager.Instance._nextTilePosition = _steppingTileQueue.First().transform.position;
        }
        else
        {
            Debug.LogError("Stepping with no stepping tiles");
        }
    }

    private void RemoveTile()
    {
        Tile lastRemovedTile = _usedTileQueue.First();
        _usedTileQueue.RemoveFirst();
        lastRemovedTile.gameObject.SetActive(false);
        _currentlyAvailableTiles[lastRemovedTile._listIndex].Enqueue(lastRemovedTile);
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
    }

    private void SetNewTileGroup()
    {
        _currentTileGroup = _tileGroups[UnityEngine.Random.Range(0, _tileGroups.Count)].PositionedTilePrefabs;
        _groupAnchorPosition = _lastPosition;
        _tileGroupStepIndex = 0;
    }

    private void PlaceNextTileSet() // Use later for shapes?
    {
        // Call PlaceNextTileSet before calling placenexttile
        // Use the placed set as a blueprint for the next X tiles

    }


}

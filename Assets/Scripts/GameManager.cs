using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    [Header("References")]
    // Replace with player script or whatever
    [SerializeField]
    private Transform _player = null;
    public Transform Player { get { return _player; } }

    [SerializeField]
    private TileGenerator _tileGenerator = null;

    [NonSerialized]
    public Vector3 _nextTilePosition = Vector3.negativeInfinity;
    [SerializeField]
    public Vector3 CurrentTilePosition = Vector3.negativeInfinity;

    public Vector3 GetNormalizedDirection()
    {
        return (_playerPathPositions[_currentlyTravelledIndex + 1].position
         - _playerPathPositions[_currentlyTravelledIndex].position).normalized;
    }
    public void SetCurrentTilePosition(Vector3 newPos)
    {
        CurrentTilePosition = newPos;
        // if (_playerPathPositions.Count != 0) return;
        // _playerPathPositions.RemoveFirst();
    }

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

    public Vector3 TileIncrement()
    {
        _currentlyTravelledIndex++;
        Debug.Log("Increment currentlyTravelled: " + _currentlyTravelledIndex);
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    public Vector3 TileDecrement()
    {
        _currentlyTravelledIndex--;
        Debug.Log("Increment currentlyTravelled: " + _currentlyTravelledIndex);
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    public int _currentlyTravelledIndex = 0;

    public Vector3 GetCurrentTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex].position;
    }

    public bool GetNextTileInteractable()
    {
        Debug.Log("CurrentlyTravelledNext: " + _currentlyTravelledIndex);
        return _playerPathPositions[_currentlyTravelledIndex].hasInteractable;
    }

    public Vector3 GetNextTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex + 1].position;
    }

    [Header("Settings")]
    public int _interactablePoolSize = 20;
    public float _jumpBaseTiming = 0f;

    public void Awake()
    {
        Instance = this;
        if (_player == null)
        {
            Debug.LogError("Player ref empty");
            _player = GetComponent<PlayerMovement>()?.transform;
        }

        if (_tileGenerator == null)
        {
            _tileGenerator = GetComponent<TileGenerator>();
        }
    }

    public void Start()
    {
        // Add to navigation later
        _tileGenerator.FullInitialization();
    }
}

#if UNITY_EDITOR


[UnityEditor.CustomEditor(typeof(GameManager))]
public class JumpDataSyncer : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gameManager = (GameManager)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Sync Gap Values"))
        {
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            gameManager._jumpBaseTiming = playerMovement.movementTweenSpeed;
        }
    }
}

#endif
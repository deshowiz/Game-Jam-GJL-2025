using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return (_playerPathPositions[_currentlyTravelledIndex + 1] - _playerPathPositions[_currentlyTravelledIndex]).normalized;
    }
    public void SetCurrentTilePosition(Vector3 newPos)
    {
        CurrentTilePosition = newPos;
        // if (_playerPathPositions.Count != 0) return;
        // _playerPathPositions.RemoveFirst();
    }

    [NonSerialized]
    public List<Vector3> _playerPathPositions = new List<Vector3>();

    public Vector3 TileIncrement()
    {
        _currentlyTravelledIndex++;
        return _playerPathPositions[_currentlyTravelledIndex];
    }

    public int _currentlyTravelledIndex = 0;

    public Vector3 GetCurrentTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex];
    }

    public Vector3 GetNextTilePos()
    {
        return _playerPathPositions[_currentlyTravelledIndex + 1];
    }

    [Header("Settings")]
    public int _interactablePoolSize = 20;

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

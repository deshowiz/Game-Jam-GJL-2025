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
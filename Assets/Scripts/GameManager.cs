using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    [Header("References")]
    // Replace with player script or whatever
    [SerializeField]
    private Player _player = null;
    public Player Player { get { return _player; } }

    [SerializeField]
    private TileGenerator _tileGenerator = null;

    [NonSerialized]
    public Vector3 _nextTilePosition = Vector3.negativeInfinity;
    [SerializeField]
    public Vector3 CurrentTilePosition = Vector3.negativeInfinity;

    [Header("Settings")]
    public int _interactablePoolSize = 20;
    public float _jumpBaseTiming = 0f;

    private int _currentBiome = -1;

    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        // Add to navigation later
        Initialize();
    }

    public void Initialize()
    {
        if (_player == null)
        {
            //Debug.LogError("Player ref empty");
            _player = GetComponent<Player>();
        }

        if (_tileGenerator == null)
        {
            _tileGenerator = GetComponent<TileGenerator>();
        }
        
        Instantiate(Resources.Load<GameObject>("GameMenu"), Vector3.zero, Quaternion.identity);
        
        NextBiome();
    }

    public void LoadNextBiome()
    {
        Debug.Log("Switching Scenes?");
        LoadNewScene();
    }

    public async Task LoadNewScene()
    {
        // call transition, await, and then await scene swap
        Debug.Log("Switching Scenes?");
        await SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);

        Initialize();
    }

    public void NextBiome()
    {
        _currentBiome++;
        if (_currentBiome == 3)
        {
            // You win screen
        }
        else
        {
            _tileGenerator.FullInitialization(0);
            //_tileGenerator.FullInitialization(_currentBiome);
        }
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
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
    public UIMinotaurBar UIMinotaurBar;
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
        
        GameObject gameMenu = Instantiate(Resources.Load<GameObject>("GameMenu"), Vector3.zero, Quaternion.identity);
        GameObject minotaurBar = Instantiate(Resources.Load<GameObject>("MinotaurDistanceCanvas"), Vector3.zero, Quaternion.identity);
        UIMinotaurBar = minotaurBar.GetComponent<UIMinotaurBar>();

        if (Application.isEditor && SceneManager.GetActiveScene().name == "PathTestSceneShawn")
        {
            //Spawn AudioManager if Editor and ShawnScene because AudioManager should spawn in MainMenu.
            GameObject audioManager = Instantiate(Resources.Load<GameObject>("AudioManager"), Vector3.zero, Quaternion.identity);
            AudioManager audio = audioManager.GetComponent<AudioManager>();
            audio.PlayMusic("WAV_GJLSpringJam2025_AMB_Cave");
        }
        
        NextBiome();
    }

    public void LoadNextBiome()
    {
        Debug.Log("Switching Scenes?");
        LoadNewScene();
    }

    public void RestartScene()
    {
        LoadNewScene();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void LoadNewScene()
    {
        // call transition, await, and then await scene swap
        SceneManager.LoadScene(0, LoadSceneMode.Single);

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
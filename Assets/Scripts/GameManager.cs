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
    public GameEvent FadeOutEvent;
    public UIMinotaurBar UIMinotaurBar;

    public GameObject playerPrefab;
    // Replace with player script or whatever
    [SerializeField]
    private Player _player = null;
    public Player Player { get { return GameObject.FindWithTag("Player").GetComponent<Player>(); } set { } }

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

    private bool _isLoading = false;
    private bool _isInitialized = false;

    public void Awake()
    {
        if (FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length != 2)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        _currentBiome = 0;
        Initialize();
    }

    public void Update()
    {
        if (_player == null)
        {
            //Debug.LogError("Player ref empty");
            //_player = GetComponent<Player>();
            _player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        }
    }

    public void Initialize()
    {
        //Instantiate(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation);
        
        if (_player == null)
        {
            //Debug.LogError("Player ref empty");
            //_player = GetComponent<Player>();
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        _tileGenerator = GameObject.FindAnyObjectByType<TileGenerator>();

        GameObject gameMenu = Instantiate(Resources.Load<GameObject>("GameMenu"), Vector3.zero, Quaternion.identity);

        if (UIMinotaurBar == null)
        {
            GameObject minotaurBar = Instantiate(Resources.Load<GameObject>("MinotaurDistanceCanvas"), Vector3.zero, Quaternion.identity);
            UIMinotaurBar = minotaurBar.GetComponent<UIMinotaurBar>();
        }

        if (Application.isEditor && SceneManager.GetActiveScene().name == "PathTestSceneShawn")
        {
            //Spawn AudioManager if Editor and ShawnScene because AudioManager should spawn in MainMenu.
            GameObject audioManager = Instantiate(Resources.Load<GameObject>("AudioManager"), Vector3.zero, Quaternion.identity);
        }
       
        _tileGenerator.FullInitialization(_currentBiome);
        _isLoading = false;
    }

    public void PleaseMinotaurBar()
    {
        GameObject minotaurBar = Instantiate(Resources.Load<GameObject>("MinotaurDistanceCanvas"), Vector3.zero, Quaternion.identity);
        UIMinotaurBar = minotaurBar.GetComponent<UIMinotaurBar>();
    }

    public void LoadNextBiome()
    {
        if (_isLoading) return;
        _isLoading = true;
        LoadNewSceneDelayed();
    }

    public void RestartScene()
    {
        if (_isLoading) return;
        _isLoading = true;
        ReloadNewSceneDelayed();
    }

    public async Task ReloadNewSceneDelayed()
    {
        await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("reload scene");
        Initialize();
    }

    public async Task LoadNewSceneDelayed()
    {
        FadeOutEvent?.Raise();
        //yield return new WaitForSeconds(1f);
        await Task.Delay(1000);
        _currentBiome++;
        if (_currentBiome == 3)
        {
            _currentBiome = 0;
        }
        await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("new scene");
        Initialize();
    }

    // public void LoadNewScene()
    // {
    //     _isLoading = true;
    //     //StartCoroutine(LoadNewSceneDelayed());
    //     LoadNewSceneDelayed();
    // }

    public void NextBiome()
    {
        _currentBiome++;

        switch (_currentBiome)
        {
            case 0:
                AudioManager.Instance.PlayMusic("WAV_GJLSpringJam2025_AMB_SpookyLevel");
                break;
            case 1:
                AudioManager.Instance.PlayMusic("WAV_GJLSpringJam2025_AMB_SpookyLevel");
                break;
            case 2:
                AudioManager.Instance.PlayMusic("WAV_GJLSpringJam2025_AMB_DeepHum");
                break;
        }
        
        if (_currentBiome == 3)
        {
            // You win screen
        }
        else
        {
            
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
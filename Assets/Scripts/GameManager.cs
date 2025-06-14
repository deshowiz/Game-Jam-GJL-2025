using System;
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
    public Vector3 CurrentTilePosition = Vector3.negativeInfinity;

    public void Awake()
    {
        Instance = this;
        if (_player == null)
        {
            Debug.LogError("Player ref empty");
            //_player = GetComponent<Player>(); for when player has a monobehaviour
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

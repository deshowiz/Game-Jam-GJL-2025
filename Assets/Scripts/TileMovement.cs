using Unity.VisualScripting;
using UnityEngine;
using DigitalRuby.Tween;

public class TileMovement : MonoBehaviour
{
    [Header("References")]
    private CameraFollow _cameraFollow = null;
    [SerializeField]
    private OrbRotater _orbRotater = null;
    [SerializeField]
    private GameEvent _steppedEvent = null;
    [Header("Settings")]
    [SerializeField]
    private float _moveDistance = 1f;
    [SerializeField]
    private KeyCode _movementKey0 = KeyCode.A;
    [SerializeField]
    private KeyCode _movementKey1 = KeyCode.D;

    private Vector3 _nextTargetPosition = Vector3.negativeInfinity;

    public void Awake()
    {
        if (_cameraFollow == null)
        {
            _cameraFollow = GameObject.FindFirstObjectByType<CameraFollow>();
        }
    }

    public void Update()
    {
        _orbRotater.SetRadius(DistanceToNextTile());
        
        if (Input.GetKeyDown(_movementKey0) || Input.GetKeyDown(_movementKey1))
        {
            int keyNum = 0;
            if (Input.GetKeyDown(_movementKey1))
            {
                keyNum = 1;
            }
            Vector3 newTargetPosition = GameManager.Instance._nextTilePosition;
            
            if (newTargetPosition.x == Mathf.NegativeInfinity)
            {
                Debug.LogError("Position not set yet?");
                return;
            }
            // _targetPosition += Vector3.right;

            if (_orbRotater.CheckOrbAccuracy(keyNum) != 0)
            {
                Vector3 targetPos = new Vector3(newTargetPosition.x, GameManager.Instance.Player.position.y,
                    newTargetPosition.z);
                
                gameObject.Tween("PlayerMove", 
                    GameManager.Instance.Player.position,
                    targetPos, 
                    0.15f,
                    TweenScaleFunctions.CubicEaseIn,
                    (t) => GameManager.Instance.Player.position = t.CurrentValue
                );
                
                //GameManager.Instance.Player.position = new Vector3(newTargetPosition.x, GameManager.Instance.Player.position.y, newTargetPosition.z);
                
                _cameraFollow._startShifting = true;
                _steppedEvent.Raise();
            }
            else // Missed
            {
                Debug.Log("Missed");
            }
        }
    }

    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePosition = GameManager.Instance.CurrentTilePosition;
        Vector3 nextTilePosition = GameManager.Instance._nextTilePosition;
        return Vector3.Distance(currentTilePosition, nextTilePosition);
    }
}
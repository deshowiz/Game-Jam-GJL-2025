using Unity.VisualScripting;
using UnityEngine;

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
    //private KeyCode _movementKey1 = KeyCode.D;

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
        if (Input.GetKeyDown(_movementKey0))
        {
            Vector3 newTargetPosition = GameManager.Instance._nextTilePosition;
            if (newTargetPosition.x == Mathf.NegativeInfinity)
            {
                Debug.LogError("Position not set yet?");
                return;
            }
            // _targetPosition += Vector3.right;

            if (_orbRotater.CheckOrbAccuracy() != 0)
            {
                GameManager.Instance.Player.position = newTargetPosition;
                _cameraFollow._startShifting = true;
                _steppedEvent.Raise();
            }
            else // Missed
            {
                Debug.Log("Missed");
            }
            
        }
    }
}

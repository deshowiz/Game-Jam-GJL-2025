using Unity.VisualScripting;
using UnityEngine;
using DigitalRuby.Tween;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Vector3 _targetCameraOffset = Vector3.zero;

    [Header("Settings")]
    [SerializeField]
    private float _cameraFollowTime = 0.5f;

    public bool _startShifting = false;

    // public void Start()
    // {
    //     Debug.Log(Camera.main.transform.position - GameManager.Instance.transform.position);
    // }

    void LateUpdate()
    {
        //this.transform.position = GameManager.Instance.Player.position + _targetCameraOffset;
        if (_startShifting)
        {
            ShiftCamera();
        }
    }

    public void StartShifting()
    {
        _startShifting = true;
    }

    private void ShiftCamera()
    {
        System.Action<ITween<Vector3>> updateCameraPos = (t) =>
            {
                this.gameObject.transform.position = t.CurrentValue;
            };

        this.gameObject.Tween("MoveCamera",
        this.gameObject.transform.position,
        GameManager.Instance.Player.transform.position + _targetCameraOffset,
        _cameraFollowTime, TweenScaleFunctions.CubicEaseIn, updateCameraPos);

        _startShifting = false;
    }
}

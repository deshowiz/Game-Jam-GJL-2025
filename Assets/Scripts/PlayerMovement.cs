using UnityEngine;
using DigitalRuby.Tween;

public class PlayerMovement : MonoBehaviour
{
    public OrbRotater orbRotater;
    public GameEvent OnPlayerStepEvent;
    
    private float movementTweenSpeed = 0.15f;
    private int stepDistance = 1;
    private float timeBetweenSteps = 0.73f;
    private float timeTillSlowDown = 5f;
    private float slowDownRate = 0.1f; //10%

    private float lastStepTime;
    private float lastTimeSlowed;
    
    private bool ready;

    private void Update()
    {
        orbRotater.SetRadius(DistanceToNextTile());
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastStepTime = Time.time;
            lastTimeSlowed = Time.time;
            ready = !ready;
        }
        
        if (!ready) return;
        
        if (Time.time >= lastStepTime + timeBetweenSteps)
        {
            Step();
        }
    }

    private void Step()
    {
        //todo: if stepDistance > 1, check which tile it is
        
        Vector3 newTargetPosition = GameManager.Instance._nextTilePosition;
        
        if (newTargetPosition.x == Mathf.NegativeInfinity)
        {
            Debug.LogError("Position not set yet?");
            return;
        }
        
        Vector3 targetPos = new Vector3(newTargetPosition.x, transform.position.y,
            newTargetPosition.z);
        
        gameObject.Tween("PlayerMove", 
            transform.position,
            targetPos, 
            movementTweenSpeed,
            TweenScaleFunctions.CubicEaseIn,
            (t) => transform.position = t.CurrentValue
        );
        
        lastStepTime += timeBetweenSteps;
        OnPlayerStepEvent.Raise();
    }
    
    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePosition = GameManager.Instance.CurrentTilePosition;
        Vector3 nextTilePosition = GameManager.Instance._nextTilePosition;
        return Vector3.Distance(currentTilePosition, nextTilePosition);
    }
}
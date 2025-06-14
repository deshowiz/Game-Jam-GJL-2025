using UnityEngine;
using DigitalRuby.Tween;

public class PlayerMovement : MonoBehaviour
{
    public GameEvent OnPlayerStepEvent;
    
    private float movementTweenSpeed = 0.15f;
    private int stepDistance = 1;
    private float timeBetweenSteps = 0.5f;
    private float slowDownRate = 0.01f;

    private float lastStepTime;
    
    private bool ready;

    private void Start()
    {
        lastStepTime = Time.time;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Step();
        }
        
        if (!ready) return;
        
        if (Time.time >= lastStepTime + timeBetweenSteps)
        {
            Step();
            lastStepTime += timeBetweenSteps;
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
        
        transform.position = targetPos;
        
        /*
        gameObject.Tween("PlayerMove", 
            transform.position,
            targetPos, 
            movementTweenSpeed,
            TweenScaleFunctions.CubicEaseIn,
            (t) => transform.position = t.CurrentValue
        );
        */
        
        OnPlayerStepEvent.Raise();
    }
}
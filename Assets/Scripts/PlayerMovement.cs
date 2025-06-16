using System.Collections;
using UnityEngine;
using DigitalRuby.Tween;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    public OrbRotater orbRotater;
    public GameEvent OnPlayerStepEvent;
    
    [Header("Settings")]
    [SerializeField]
    [Range(0f, 10f)]
    private float _baseSpeed = 1f;
    private float _boostSpeed = 0f;

    private float _stepPercentageCompleted = 0f;
    [SerializeField]
    [Range(0f, 5f)]
    private float movementTweenSpeed = 0.15f;
    private float _fullSpeed = 0f;
    private bool ready;

    private bool _jumping = false;


    // Use next position in GameManager to get move direction
    // record travel distance between interactables
    // in the beginning, ^the first interactable position will need to be the starting tile position, 0,0,0
    // frame by frame, below \\
    // take that distance and dynamically, divide how long it will take to get there
    // Use that time and take the current speed of the player and divide for number of rotations
    // round down
    private void LateUpdate()
    {
        _fullSpeed = _baseSpeed + _boostSpeed;
        float distToNext = DistanceToNextTile();
        orbRotater.SetRadius(distToNext);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ready = !ready;
        }

        if (!ready) return;
        
        if (distToNext == 1f) // Distance of greater than 1 indicates a gap since all tiles need to have a diameter of 1 or lower
        {
            Slide();
        }
        else if (!_jumping)/*if (Time.time >= lastStepTime + timeBetweenSteps)*/
        {
            Step();
        }
    }

    public void Slow(float slowPercentage)
    {
        _boostSpeed *= (1f - slowPercentage / 100f);
        _boostSpeed = Mathf.Max(_boostSpeed, 0f);
    }
    
    public void Boost(float boostPercentage)
    {
        _boostSpeed *= (1f + boostPercentage / 100f);
        _boostSpeed = Mathf.Max(_boostSpeed, 0f);
    }

    public IEnumerator Stun(float seconds)
    {
        ready = false;
        yield return new WaitForSeconds(seconds);
        ready = true;
    }

    // Changing the movement to be sliding when not originally intended is a bit scuffed structure wise
    // Getting all of the edge cases for something not originally designed for the sliding was rough
    // Would have a better solution if it didn't require refactoring a previous system set from diff expectations
    private void Slide()
    {
        Vector3 finalDestination = transform.position;
        float distanceThisFrame = _baseSpeed * Time.deltaTime;
        _stepPercentageCompleted += distanceThisFrame;

        if (_stepPercentageCompleted < 1f)
        {
            finalDestination += GameManager.Instance.GetNormalizedDirection() * distanceThisFrame;
        }
        else
        {
            do
            {
                _stepPercentageCompleted--;
                finalDestination = GameManager.Instance.TileIncrement();
                Vector3 nextTile = GameManager.Instance.GetNextTilePos();
                finalDestination = Vector3.Lerp(finalDestination, nextTile, _stepPercentageCompleted);
            }
            while (_stepPercentageCompleted > 1f);
        }
        transform.position = new Vector3(finalDestination.x, 0f, finalDestination.z);
    }

    private void Step()
    {
        //todo: if stepDistance > 1, check which tile it is

        Vector3 newTargetPosition = GameManager.Instance.GetNextTilePos();;

        if (newTargetPosition.x == Mathf.NegativeInfinity)
        {
            Debug.LogError("Position not set yet?");
            return;
        }

        Vector3 targetPos = new Vector3(newTargetPosition.x, transform.position.y,
            newTargetPosition.z);

        System.Action<ITween<Vector3>> updateNextTile = (t) =>
        {
            GameManager.Instance.TileIncrement();
            _jumping = false;
        };

        _jumping = true;
        gameObject.Tween("PlayerMove",
            transform.position,
            targetPos,
            movementTweenSpeed / _fullSpeed,
            TweenScaleFunctions.CubicEaseIn,
            (t) => transform.position = t.CurrentValue, updateNextTile
        );

        // lastStepTime += timeBetweenSteps;
        // OnPlayerStepEvent.Raise();
    }
    
    private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePosition = GameManager.Instance.CurrentTilePosition;
        Vector3 nextTilePosition = GameManager.Instance._nextTilePosition;
        return Vector3.Distance(GameManager.Instance.GetCurrentTilePos(),
         GameManager.Instance.GetNextTilePos());
    }
}
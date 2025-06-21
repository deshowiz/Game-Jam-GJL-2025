using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinotaurMovement : MonoBehaviour
{
    public GameObject teleportFx;

    [Header("References")]
    [SerializeField]
    private Transform _minotaurTransform = null;
    [Header("Settings")]
    [SerializeField]
    private float _baseSpeed = 3f;
    [SerializeField]
    private float _boostSpeed = 0f;

    private float _fullSpeed = 0f;
    [Range(0f, 5f)]
    public float movementTweenSpeed = 0.15f;
    [SerializeField]
    private Vector3 _playerEffectOffset = Vector3.zero;

    private List<Vector3> _tilesToTravel = new List<Vector3>();
    public void AddTileToTravel(Vector3 newTilePosition)
    {
        _tilesToTravel.Add(newTilePosition);
    }
    private float _stepPercentageCompleted = 0f;
    private float _currentPairDistance = 0f;
    private bool ready = false;

    private bool _jumping = false;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ready = !ready;
        }
        if (!ready || _tilesToTravel.Count == 0) return;
        _fullSpeed = _baseSpeed + _boostSpeed;
        float distToNext = DistanceToNextTile();
        if (distToNext == 1f) // Distance of greater than 1 indicates a gap since all tiles need to have a diameter of 1 or lower
        {
            Slide();
        }
        else if (!_jumping)/*if (Time.time >= lastStepTime + timeBetweenSteps)*/
        {
            Step();
        }
    }

    private void Slide()
    {
        Vector3 finalDestination = transform.position;
        float distanceThisFrame = _fullSpeed * Time.deltaTime;
        _stepPercentageCompleted += distanceThisFrame;
        _currentPairDistance = Vector3.Distance(_tilesToTravel[1], _tilesToTravel[0]);

        if (_stepPercentageCompleted < _currentPairDistance)
        {
            finalDestination +=
            Vector3.Normalize(_tilesToTravel[1] - _tilesToTravel[0]) * distanceThisFrame;
        }
        else
        {
            do
            { // In case the player moves more than one tile's space during a frame
                _stepPercentageCompleted -= _currentPairDistance;
                finalDestination = TileIncrement();
                //Time.timeScale = 0f;
                Vector3 nextTile = _tilesToTravel[1];
                if (nextTile.x == Mathf.NegativeInfinity)
                {
                    ready = false;
                    return;
                }
                finalDestination = Vector3.Lerp(finalDestination, nextTile, _stepPercentageCompleted);
                _currentPairDistance = Vector3.Distance(nextTile, _tilesToTravel[0]);
            }
            while (_stepPercentageCompleted > _currentPairDistance);
        }

        transform.position = new Vector3(finalDestination.x, finalDestination.y, finalDestination.z);
    }

    private void Step()
    {
        Vector3 newTargetPosition = _tilesToTravel[1];
        if (newTargetPosition.x == Mathf.NegativeInfinity)
        {
            Debug.LogError("Position not set yet?");
            return;
        }
        Vector3 targetPos = new Vector3(newTargetPosition.x, transform.position.y,
            newTargetPosition.z);

        StartCoroutine(Teleport(targetPos));
    }

    IEnumerator Teleport(Vector3 destination)
    {
        _jumping = true;

        yield return new WaitForSeconds(movementTweenSpeed / _fullSpeed);

        Vector3 nextTile = TileIncrement();
        _jumping = false;

        transform.position = destination;

        GameObject lol1 = Instantiate(teleportFx, transform.position + _playerEffectOffset, Quaternion.identity);
        //lol1.transform.localScale = new Vector3(1f, 1.7778f, 1);
        Destroy(lol1, 2f);
    }

    private Vector3 TileIncrement()
    {
        _tilesToTravel.RemoveAt(0);
        return _tilesToTravel[0];
    }

        private float DistanceToNextTile()
    {
        if (!GameManager.Instance) return 0.0f;
        Vector3 currentTilePos = _tilesToTravel[0];
        Vector3 nextTilePos = _tilesToTravel[1];
        return Vector2.Distance(new Vector2(currentTilePos.x, currentTilePos.z),
         new Vector2(nextTilePos.x, nextTilePos.z));
    }


}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeInteractableData", menuName = "Scriptable Objects/BiomeInteractableData")]
public class BiomeInteractableData : ScriptableObject
{
    [Header("References")]
    [SerializeField]
    private List<InteractableTile> _powerups = new List<InteractableTile>();
    [SerializeField]
    private List<InteractableTile> _traps = new List<InteractableTile>();

    private List<Queue<InteractableTile>> _availablePowerups = null;
    private Queue<InteractableTile> _activePowerups = null;

    private List<InteractableTile> _copiedPowerups = null;
    private InteractableTile _usedPowerup = null;

    private List<Queue<InteractableTile>> _availableTraps = null;
    private Queue<InteractableTile> _activeTraps = null;

    private List<InteractableTile> _copiedTraps = null;
    private InteractableTile _usedTrap = null;

    [Header("Settings")]
    [SerializeField]
    private int _boostMinimumTileSpacing = 10;
    [SerializeField]
    private int _boostMaximumTileSpacing = 20;

    public int GetNextBoostPosition()
    {
        return UnityEngine.Random.Range(_boostMinimumTileSpacing, _boostMaximumTileSpacing);
    }

    [SerializeField]
    private int _trapMinimumTileSpacing = 10;
    [SerializeField]
    private int _trapMaximumTileSpacing = 20;

    public int GetNextTrapPosition()
    {
        return UnityEngine.Random.Range(_trapMinimumTileSpacing, _trapMaximumTileSpacing);
    }
    

    public void InitializeBiome()
    {
        _availablePowerups = new List<Queue<InteractableTile>>();
        for (int i = 0; i < _powerups.Count; i++)
        {
            _availablePowerups.Add(new Queue<InteractableTile>());
            for (int j = 0; j < GameManager.Instance._interactablePoolSize; j++)
            {
                InteractableTile newPowerupCopy = Instantiate(_powerups[i]);
                newPowerupCopy._listIndex = i;
                _availablePowerups[i].Enqueue(newPowerupCopy);
            }
        }
        _activePowerups = new Queue<InteractableTile>();
        _copiedPowerups = new List<InteractableTile>(_powerups);
        _usedPowerup = null;


        _availableTraps = new List<Queue<InteractableTile>>();
        for (int i = 0; i < _traps.Count; i++)
        {
            _availableTraps.Add(new Queue<InteractableTile>());
            for (int j = 0; j < GameManager.Instance._interactablePoolSize; j++)
            {
                InteractableTile newTrapCopy = Instantiate(_traps[i]);
                newTrapCopy._listIndex = i;
                _availableTraps[i].Enqueue(newTrapCopy);
            }
        }
        _activeTraps = new Queue<InteractableTile>();
        _copiedTraps = new List<InteractableTile>(_traps);
        _usedTrap = null;
    }

    public void SetRandomPowerup(Vector3 boostPosition)
    {
        int chosenIndex = UnityEngine.Random.Range(0, _copiedPowerups.Count);
        InteractableTile chosenTile = _copiedPowerups[chosenIndex];
        InteractableTile _newActiveTile = _availablePowerups[chosenTile._listIndex].Dequeue();
        _activePowerups.Enqueue(_newActiveTile);
        _newActiveTile.transform.position = boostPosition;
        _newActiveTile.gameObject.SetActive(true);

        if (_usedPowerup != null)
        {
            _copiedPowerups.Add(_usedPowerup);
        }
        _usedPowerup = chosenTile;
        _copiedPowerups.RemoveAt(chosenIndex);
    }

    public void SetRandomTrap(Vector3 trapPosition)
    {
        int chosenIndex = UnityEngine.Random.Range(0, _copiedTraps.Count);
        InteractableTile chosenTile = _copiedTraps[chosenIndex];
        InteractableTile _newActiveTile = _availableTraps[chosenTile._listIndex].Dequeue();
        _activeTraps.Enqueue(_newActiveTile);
        _newActiveTile.transform.position = trapPosition;
        _newActiveTile.gameObject.SetActive(true);

        if (_usedTrap != null)
        {
            _copiedTraps.Add(_usedTrap);
        }
        _usedTrap = chosenTile;
        _copiedTraps.RemoveAt(chosenIndex);
    }
}

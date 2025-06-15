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

    private List<List<InteractableTile>> _availablePowerups = null;
    private Queue<InteractableTile> _activePowerups = null;

    private List<InteractableTile> _copiedPowerups = null;
    private InteractableTile _usedPowerup = null;

    private List<InteractableTile> _copiedTraps = null;
    private InteractableTile _usedTrap = null;

    [Header("Settings")]
    [SerializeField]
    private int _boostMinimumTileSpacing = 10;
    [SerializeField]
    private int _boostMaximumTileSpacing = 20;

    public int GetNextBoost()
    {
        return UnityEngine.Random.Range(_boostMinimumTileSpacing, _boostMaximumTileSpacing);
    }

    [SerializeField]
    private int _trapMinimumTileSpacing = 10;
    [SerializeField]
    private int _trapMaximumTileSpacing = 20;

    public int GetNextTrap()
    {
        return UnityEngine.Random.Range(_trapMinimumTileSpacing, _trapMaximumTileSpacing);
    }
    

    public void InitializeBiome()
    {
        _availablePowerups = new List<List<InteractableTile>>();
        _activePowerups = new Queue<InteractableTile>();


        _copiedPowerups = new List<InteractableTile>(_powerups);
        _usedPowerup = null;
        _copiedTraps = new List<InteractableTile>(_traps);
        _usedTrap = null;
    }

    public InteractableTile GetRandomPowerup()
    {
        int chosenIndex = UnityEngine.Random.Range(0, _copiedPowerups.Count);
        InteractableTile chosenTile = _copiedPowerups[chosenIndex];
        if (_usedPowerup != null)
        {
            _copiedPowerups.Add(_usedPowerup);
        }
        _usedPowerup = chosenTile;
        _copiedPowerups.RemoveAt(chosenIndex);
        return chosenTile;
    }

    public InteractableTile GetRandomTrap()
    {
        int chosenIndex = UnityEngine.Random.Range(0, _copiedTraps.Count);
        InteractableTile chosenTile = _copiedTraps[chosenIndex];
        if (_usedTrap != null)
        {
            _copiedTraps.Add(_usedTrap);
        }
        _usedTrap = chosenTile;
        _copiedTraps.RemoveAt(chosenIndex);
        return chosenTile;
    }
}

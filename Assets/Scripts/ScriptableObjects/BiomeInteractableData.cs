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
}

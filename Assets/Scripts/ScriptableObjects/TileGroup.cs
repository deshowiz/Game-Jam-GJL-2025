using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileGroup", menuName = "Scriptable Objects/TileGroup")]
public class TileGroup : ScriptableObject
{
    // [Header("Ordered Tile References")]
    [SerializeField]
     private List<PositionedGroup> _positionedTilePrefabs = new List<PositionedGroup>();
    public List<PositionedGroup> PositionedTilePrefabs { get { return _positionedTilePrefabs; } }

    [Serializable]
    public struct PositionedGroup
    {
        public Tile tilePrefab;
        public Vector3 position; // local, add offset
    }
}

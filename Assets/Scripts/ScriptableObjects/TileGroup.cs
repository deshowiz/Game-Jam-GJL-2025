using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
#if UNITY_EDITOR


[UnityEditor.CustomEditor(typeof(TileGroup))]
public class GroupSpawner : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TileGroup tileGroup = (TileGroup)target;
        const string currentGroupHolder = "CurrentGroupHolder";

        if (GUILayout.Button("Rebuild Group"))
        {
            GameObject currentHolder = GameObject.Find(currentGroupHolder);

            if (currentHolder != null)
            {
                DestroyImmediate(currentHolder);
            }

            GameObject newHolderT = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
            newHolderT.name = currentGroupHolder;

            List<TileGroup.PositionedGroup> groupToSpawn = tileGroup.PositionedTilePrefabs;

            for (int i = 0; i < groupToSpawn.Count; i++)
            {
                Tile currentTile = Instantiate(groupToSpawn[i].tilePrefab, groupToSpawn[i].position, Quaternion.identity, newHolderT.transform);
                currentTile.gameObject.SetActive(true);
            }

            GameObject lastExtra = GameObject.Find("New Game Object");
            if (lastExtra != null)
            {
                DestroyImmediate(lastExtra);
            }
        }

        if (GUILayout.Button("Remove Group"))
        {
            GameObject currentHolder = GameObject.Find(currentGroupHolder);

            if (currentHolder != null)
            {
                DestroyImmediate(currentHolder);
            }
        }

    }
}
#endif
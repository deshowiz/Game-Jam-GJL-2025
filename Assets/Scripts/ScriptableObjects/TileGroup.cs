using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "TileGroup", menuName = "Scriptable Objects/TileGroup")]
public class TileGroup : ScriptableObject
{
    #if UNITY_EDITOR
    [Header("Tile Data")]
    [SerializeField]
    private Tile _baseTilePrefab = null;
    public Tile BaseTilePrefab {get{ return _baseTilePrefab; }}
    #endif

    [Header("Wall Data")]
    [SerializeField]
    private Wall _baseWallPrefab = null;
    public Wall BaseWallPrefab { get { return _baseWallPrefab;}}

    [SerializeField]
    private List<WallPositionList> _wallSectionVariations;
    public List<WallPositionList> WallSectionVariations{get{ return _wallSectionVariations;}}

    public int WallSectionCount {get{ return _wallSectionVariations.Count; }}

    [Serializable]
    public struct WallPositionList
    {
        public List<WallData> wallDataList; // local, add offset
    }

    [Serializable]
    public struct WallData
    {
        public Mesh wallMeshPrefab;
        public Vector3 position;
    }

    public List<WallData> WallSectionVariation(int directionIndex)
    {
        return _wallSectionVariations[directionIndex].wallDataList;
    }

    [Header("Group Data")]
    [SerializeField]
    private List<PositionedGroup> _positionedTilePrefabs = new List<PositionedGroup>();
    public List<PositionedGroup> PositionedTilePrefabs { get { return _positionedTilePrefabs; } }

    [Serializable]
    public struct PositionedGroup
    {
        public Mesh tileMeshPrefab;
        public Vector3 position; // local, add offset
    }

    
}
#if UNITY_EDITOR


[UnityEditor.CustomEditor(typeof(TileGroup))]
public class GroupSpawner : Editor
{
    public override void OnInspectorGUI()
    {

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

            GameObject lastExtra = GameObject.Find("New Game Object");
            if (lastExtra != null)
            {
                DestroyImmediate(lastExtra);
            }

            List<TileGroup.PositionedGroup> groupToSpawn = tileGroup.PositionedTilePrefabs;

            for (int i = 0; i < groupToSpawn.Count; i++)
            {
                Tile currentTile = Instantiate(tileGroup.BaseTilePrefab, groupToSpawn[i].position, Quaternion.identity, newHolderT.transform);
                currentTile.SetMesh(groupToSpawn[i].tileMeshPrefab);
                currentTile.gameObject.SetActive(true);
            }

            UnityEngine.Random.InitState(DateTime.Now.Millisecond);

            GameObject newWallPrefab = tileGroup.BaseWallPrefab.gameObject;
            
            List<TileGroup.WallData> currentWallSection = tileGroup.WallSectionVariation(UnityEngine.Random.Range(0, tileGroup.WallSectionVariations.Count));

            for (int j = 0; j < currentWallSection.Count; j++)
            {
                GameObject currentWall = Instantiate(newWallPrefab, currentWallSection[j].position, Quaternion.identity, newHolderT.transform);
                currentWall.SetActive(true);
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
        DrawDefaultInspector();
    }
}
#endif
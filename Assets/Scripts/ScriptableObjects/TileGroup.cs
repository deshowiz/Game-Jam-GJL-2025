using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "TileGroup", menuName = "Scriptable Objects/TileGroup")]
public class TileGroup : ScriptableObject
{
    [Header("Wall Data")]
    [SerializeField]
    private Tile _wallPrefab;
    public Tile WallPrefab {get{ return _wallPrefab; }}

    [SerializeField]
    private List<WallPositionList> _wallSectionVariations;
    public List<WallPositionList> WallSectionVariations{get{ return _wallSectionVariations;}}

    public int WallSectionCount {get{ return _wallSectionVariations.Count; }}

    [Serializable]
    public struct WallPositionList
    {
        public List<Vector3> positions; // local, add offset
    }

    public List<Vector3> WallSectionVariation(int directionIndex)
    {
        return _wallSectionVariations[directionIndex].positions;
    }

    
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
                Tile currentTile = Instantiate(groupToSpawn[i].tilePrefab, groupToSpawn[i].position, Quaternion.identity, newHolderT.transform);
                currentTile.gameObject.SetActive(true);
            }

            UnityEngine.Random.InitState(DateTime.Now.Millisecond);

            GameObject newWallPrefab = tileGroup.WallPrefab.gameObject;
            
            List<Vector3> currentWallSection = tileGroup.WallSectionVariation(UnityEngine.Random.Range(0, tileGroup.WallSectionVariations.Count));

            for (int j = 0; j < currentWallSection.Count; j++)
            {
                GameObject currentWall = Instantiate(newWallPrefab, currentWallSection[j], Quaternion.identity, newHolderT.transform);
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
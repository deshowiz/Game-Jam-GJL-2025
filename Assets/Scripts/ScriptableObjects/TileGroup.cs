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
    public Tile BaseTilePrefab { get { return _baseTilePrefab; } }
#endif

    [Header("Wall Data")]
    [SerializeField]
    private Wall _baseWallPrefab = null;
    public Wall BaseWallPrefab { get { return _baseWallPrefab; } }

    [SerializeField]
    private WallPositionList _wallSection;
    public WallPositionList WallSection { get { return _wallSection; } }

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

    // public List<WallData> WallSectionVariation(int directionIndex)
    // {
    //     return _wallSectionVariations[directionIndex].wallDataList;
    // }

    [Header("Group Data")]
    [SerializeField]
    private List<PositionedGroup> _positionedTilePrefabs = new List<PositionedGroup>();
    public List<PositionedGroup> PositionedTilePrefabs { get { return _positionedTilePrefabs; } }

    [Serializable]
    public struct PositionedGroup
    {
        [SerializeField]
        public Mesh tileMeshPrefab;
        [SerializeField]
        public Vector3 position; // local, add offset

        public PositionedGroup(Mesh newMesh, Vector3 newPosition)
        {
            this.tileMeshPrefab = newMesh;
            this.position = newPosition;
        }
    }

#if UNITY_EDITOR
    public void ScanForDuplicatePairs()
    {
        for (int i = 0; i < _positionedTilePrefabs.Count; i++)
        {
            for (int j = 0; j < _positionedTilePrefabs.Count; j++)
            {
                if (i != j)
                {
                    if (_positionedTilePrefabs[i].position == _positionedTilePrefabs[j].position)
                    {
                        Debug.Log("Duplicate found for " + this.name + " at index " + i + ", please remove duplicate manually");
                    }
                }
                
            }
        }
    }
    #endif

    
}
#if UNITY_EDITOR


[UnityEditor.CustomEditor(typeof(TileGroup))]
public class GroupSpawner : Editor
{
    public override void OnInspectorGUI()
    {

        TileGroup tileGroup = (TileGroup)target;
        string currentGroupHolder = "CurrentGroupHolder";

        if (GUILayout.Button("Rebuild Group"))
        {
            Rebuild(tileGroup, currentGroupHolder);
        }

        if (GUILayout.Button("Remove Group"))
        {
            GameObject currentHolder = GameObject.Find("CurrentGroupHolder");

            if (currentHolder != null)
            {
                DestroyImmediate(currentHolder);
            }
        }

        // if (GUILayout.Button("Fill Empty Tile Mesh References"))
        // {
        //     // Mesh cubeMesh = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("0000000000000000e000000000000000"), typeof(Mesh)) as Mesh;
        //     // Debug.Log(cubeMesh);
        //     List<TileGroup.PositionedGroup> positionedTiles = tileGroup.PositionedTilePrefabs;
        //     Mesh cubeMesh = Resources.GetBuiltinResource(typeof(Mesh), "Cube.fbx") as Mesh;
        //     Debug.Log(cubeMesh);
        //     for (int i = 0; i < tileGroup.PositionedTilePrefabs.Count; i++)
        //     {
        //         Vector3 copiedPosition = positionedTiles[i].position;
        //         positionedTiles[i] = new TileGroup.PositionedGroup(cubeMesh, copiedPosition);
        //     }
        //     EditorUtility.SetDirty(this);
        //     Debug.Log(EditorUtility.IsDirty(this));
        //     Undo.RecordObject(this, "Setting Cube Meshes");
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        // }


        DrawDefaultInspector();


        if (GUILayout.Button("Rebuild Group"))
        {
            Rebuild(tileGroup, currentGroupHolder);
        }

        if (GUILayout.Button("Remove Group")) // Had to repeat this code cause of an editor bug
        {
            GameObject currentHolder = GameObject.Find("CurrentGroupHolder");

            if (currentHolder != null)
            {
                DestroyImmediate(currentHolder);
            }
        }

        if (GUILayout.Button("Scan All Groups")) // Had to repeat this code cause of an editor bug
        {
            string[] tileGroupGUIDs = AssetDatabase.FindAssets("t:TileGroup");
            //Debug.Log(tileGroupGUIDs.Length);
            for (int i = 0; i < tileGroupGUIDs.Length; i++)
            {
                TileGroup currentGroup = AssetDatabase.LoadAssetAtPath<TileGroup>(AssetDatabase.GUIDToAssetPath(tileGroupGUIDs[i]));
                currentGroup.ScanForDuplicatePairs();
            }
        }
    }

    private void Rebuild(TileGroup tileGroup, string currentGroupHolder)
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

        Wall newWallPrefab = tileGroup.BaseWallPrefab;

        List<TileGroup.WallData> currentWallSection = tileGroup.WallSection.wallDataList;

        for (int j = 0; j < currentWallSection.Count; j++)
        {
            Wall currentWall = Instantiate(newWallPrefab, currentWallSection[j].position, Quaternion.identity, newHolderT.transform);
            currentWall.SetMesh(currentWallSection[j].wallMeshPrefab);
            currentWall.gameObject.SetActive(true);
        }
    }
    

}
#endif
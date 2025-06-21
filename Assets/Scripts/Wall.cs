using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _wallMeshFilter;
    public void SetMesh(Mesh meshPrefab)
    {
        _wallMeshFilter.mesh = meshPrefab;
    }
}

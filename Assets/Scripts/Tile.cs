using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter = null;
    public void SetMesh(Mesh newMesh)
    {
        _meshFilter.mesh = newMesh;
    }
}

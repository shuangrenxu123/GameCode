using UnityEngine;

public class Chunk3D : MonoBehaviour
{
    /// <summary>
    /// 区块的区块坐标
    /// </summary>
    public Vector3Int coord;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshCollider meshcollider;
    MeshRenderer meshrenderer;
    public void DestroyOrDisable()
    {
        if (Application.isPlaying)
        {
            mesh.Clear();
            gameObject.SetActive(false);
        }
        else
        {
            DestroyImmediate(gameObject, false);
        }
    }
    public void SetColl(bool a)
    {
        meshcollider.enabled = a;
    }
    public void SetUp(Material material)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshrenderer = GetComponent<MeshRenderer>();
        meshcollider = GetComponent<MeshCollider>();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshrenderer == null)
        {
            meshrenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (meshcollider == null)
        {
            meshcollider = gameObject.AddComponent<MeshCollider>();
        }
        mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        if (meshcollider.sharedMesh == null)
        {
            meshcollider.sharedMesh = mesh;
        }
        meshcollider.enabled = false;
        meshcollider.enabled = true;

        meshrenderer.material = material;
    }
    public void UpdateMesh(MeshData data)
    {
        mesh.Clear();
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.RecalculateNormals();
        meshcollider.enabled = false;
        meshcollider.enabled = true;
    }
}
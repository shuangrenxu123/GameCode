using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    [Header("Collider")]
    public bool addMeshCollider;
    [Header("Material")]
    public Material material;
    public bool ShowBoungdingBox = false;
    public bool autoUpdate = true;
    public MeshInfo meshInfo;

    protected MeshCollider meshCollider;
    protected MeshRenderer meshRenderer;
    protected MeshFilter meshFilter;
    protected Mesh mesh;

    private void Start()
    {
        Generate();
        meshInfo = new MeshInfo(mesh);
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }
    private void OnValidate()
    {
        if (autoUpdate)
        {
            Generate();
            //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshInfo = new MeshInfo(mesh);
        }
        else
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
    }
    /// <summary>
    /// 创建mesh
    /// </summary>
    protected virtual void Generate()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.sharedMesh = mesh;
    }
    protected void ConfigCollider()
    {
        if (addMeshCollider)
        {
            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = gameObject.AddComponent<MeshCollider>();
                }
                meshCollider.enabled = true;
            }
            else
            {
                meshCollider = GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    meshCollider.enabled = false;
                }
            }
        }
    }

}
//程序生成mesh
public class ProceduralMeshPart
{
    /// <summary>
    /// mesh顶点
    /// </summary>
    private List<Vector3> vertices = new List<Vector3>();
    /// <summary>
    /// 每三个为一组来连接为一个面片，下标则为mesh中的顶点顺序
    /// </summary>
    private List<int> triangeles = new List<int>();

    private List<Color> colors = new List<Color>();

    public Vector3[] vers;
    public int[] tris;
    public Color[] color;

    public void FillArray()
    {
        vers = vertices.ToArray();
        tris = triangeles.ToArray();
        color = colors.ToArray();
    }
    public ProceduralMeshPart()
    {

    }
    public ProceduralMeshPart(Vector3[] ver, int[] tints, Color[] c)
    {
        vers = ver;
        tris = tints;
        color = c;
    }
    /// <summary>
    /// 添加一个三角形面片
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="point3"></param>
    public void AddTriangle(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        int vertexindex = vertices.Count;
        vertices.Add(point1);
        vertices.Add(point2);
        vertices.Add(point3);
        triangeles.Add(vertexindex);
        triangeles.Add(vertexindex + 1);
        triangeles.Add(vertexindex + 2);
    }
    /// <summary>
    /// 添加一个四边形
    /// </summary>
    /// <param name="ld">左下</param>
    /// <param name="lt">左上</param>
    /// <param name="rt">右上</param>
    /// <param name="rd">右下</param>
    public void AddQuad(Vector3 ld, Vector3 lt, Vector3 rt, Vector3 rd)
    {
        int count = vertices.Count;
        vertices.Add(ld);
        vertices.Add(lt);
        vertices.Add(rt);
        vertices.Add(rd);
        triangeles.Add(count);
        triangeles.Add(count + 1);
        triangeles.Add(count + 2);
        triangeles.Add(count);
        triangeles.Add(count + 2);
        triangeles.Add(count + 3);
    }
    /// <summary>
    /// 创建一个五边形
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="e"></param>
    public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int count = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        vertices.Add(e);
        triangeles.Add(count);
        triangeles.Add(count + 1);
        triangeles.Add(count + 2);
        triangeles.Add(count);
        triangeles.Add(count + 2);
        triangeles.Add(count + 3);
        triangeles.Add(count);
        triangeles.Add(count + 3);
        triangeles.Add(count + 4);
    }
    public void AddTriangle(Vector3 point1, Vector3 point2, Vector3 point3, float height)
    {
        Vector3 h = new Vector3(0, height, 0);
        Vector3 point1h = point1 + h;
        Vector3 point2h = point2 + h;
        Vector3 point3h = point3 + h;
        AddTriangle(point1, point3, point2);
        AddTriangle(point1h, point2h, point3h);
        AddQuad(point1, point2, point2h, point1h);
        AddQuad(point2, point3, point3h, point2h);
        AddQuad(point3, point1, point1h, point3h);
    }

    public void AddQuad(Vector3 lb, Vector3 lt, Vector3 rt, Vector3 rb, float height)
    {
        Vector3 h = new Vector3(0, height, 0);
        Vector3 lbh = lb + h;
        Vector3 lth = lt + h;
        Vector3 rth = rt + h;
        Vector3 rbh = rb + h;
        AddQuad(lb, rb, rt, lt);
        AddQuad(lbh, lth, rth, rbh);
        AddQuad(lb, lbh, rbh, rb);
        AddQuad(rb, rbh, rth, rt);
        AddQuad(rt, rth, lth, lt);
        AddQuad(lt, lth, lbh, lb);
    }

    public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e, float height)
    {
        Vector3 h = new Vector3(0, height, 0);
        Vector3 ah = a + h;
        Vector3 bh = b + h;
        Vector3 ch = c + h;
        Vector3 dh = d + h;
        Vector3 eh = e + h;
        AddPentagon(a, e, d, c, b);
        AddPentagon(ah, bh, ch, dh, eh);
        AddQuad(a, ah, eh, e);
        AddQuad(e, eh, dh, d);
        AddQuad(d, dh, ch, c);
        AddQuad(c, ch, bh, b);
        AddQuad(b, bh, ah, a);
    }
}
[Serializable]
public class MeshInfo
{
    public int verticeCount;
    public int triangleCount;

    public MeshInfo(Mesh mesh)
    {
        verticeCount = mesh.vertexCount;
        triangleCount = mesh.triangles.Length / 3;
    }
}
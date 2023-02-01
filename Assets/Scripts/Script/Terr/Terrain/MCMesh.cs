using UnityEngine;

public class MCMesh : Chunk3DManager
{
    //每一个线程组中的线程的数量
    const int threadGroupSize = 8;
    [Header("生成设置")]
    public ComputeShader CS;
    //x轴上的线程数量
    public int numPointsPerAxis = 16;
    public float isoLevel = -0.5f;

    public NoiseTerrainGenerator value;

    //public D value;
    private ComputeBuffer pointsBuffer;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triangleCountBuffer;

    protected override void UpdateChunkMesh(Chunk3D chunk)
    {
        CreateBuffer();
        chunk.UpdateMesh(CreateMesh(chunk));
    }
    /// <summary>
    /// 创建cs中所使用到的缓冲区
    /// </summary>
    private void CreateBuffer()
    {
        int numspoint = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxes = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxes * 5;

        if (pointsBuffer == null || pointsBuffer.count != numspoint)
        {
            ReleaseBuffer();
            pointsBuffer = new ComputeBuffer(numspoint, sizeof(float) * 4);
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }
    }

    private MeshData CreateMesh(Chunk3D chunk)
    {
        //因为从0开始，所以要减去1
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        //线程组的数量
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)threadGroupSize);
        value.Generate(pointsBuffer, numPointsPerAxis, chunkSize / numVoxelsPerAxis, CenterFromCoord(chunk.coord));
        triangleBuffer.SetCounterValue(0);

        CS.SetBuffer(0, "points", pointsBuffer);
        CS.SetBuffer(0, "triangles", triangleBuffer);
        CS.SetInt("numPointsPerAxis", numPointsPerAxis);
        CS.SetFloat("isoLevel", isoLevel);

        CS.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);


        //将顶点数组存储到另一个数组里
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        int numTris = triangleCountArray[0];
        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);



        //// Get number of triangles in the triangle buffer

        // Construct Mesh
        Vector3[] vers = new Vector3[numTris * 3];
        int[] trias = new int[numTris * 3];


        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                trias[i * 3 + j] = i * 3 + j;
                vers[i * 3 + j] = tris[i][j];
            }
        }
        return new MeshData(vers, trias);
    }
    /// <summary>
    /// /刷新缓冲区
    /// </summary>
    private void ReleaseBuffer()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            pointsBuffer.Release();
            triangleCountBuffer.Release();
        }
    }
}
struct Triangle
{
#pragma warning disable 649 // 忽略变量未分配的警告
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
    public Vector3 this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                default:
                    return c;
            }
        }
    }

}

public struct MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    public MeshData(Vector3[] vertices, int[] triangles)
    {
        this.vertices = vertices;
        this.triangles = triangles;
    }
}

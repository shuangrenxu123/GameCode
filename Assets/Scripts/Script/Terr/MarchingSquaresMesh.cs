using System;
using UnityEngine;
public class MarchingSquaresMesh : ProceduralMesh
{
    public ComputeShader cs;
    public MarchingSquaresDebugSettings debugSettings;
    public bool GPU = false;
    [Header("Mesh")]
    public bool Build3D = false;
    public int x = 20;
    public int y = 20;
    public int size = 2;
    [Header("¸ß¶È")]
    public int hight = 1;
    [Range(0, 1)]
    public float threshold = 0.5f;
    public bool islerp = false;
    public int offset;
    public float scale;

    private void Start()
    {
        Generate();
    }
    private MarchingSquare[] squaresMap;
    protected override void Generate()
    {
        base.Generate();
        int length = x * y;
        float halfSize = size * 0.5f;
        squaresMap = new MarchingSquare[length];
        ProceduralMeshPart main = new ProceduralMeshPart();

        if (GPU)
        {
            int kernel = cs.FindKernel("CSMain");
            ComputeBuffer buffer = new ComputeBuffer(x * y, 4);
            cs.SetBuffer(kernel, "pointers", buffer);
            cs.SetFloat("halfsize", halfSize);
            cs.SetFloat("threshold", threshold);
            cs.SetFloat("scale", scale);
            cs.Dispatch(kernel, 1, 1, 1);

            var values = new point[x * y];
            buffer.GetData(values);

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    int index = i * y + j;
                    squaresMap[index] = new MarchingSquare(values[index].value, new Vector3(i * size + halfSize, 0, j * size + halfSize), halfSize);
                    if (Build3D)
                    {
                        squaresMap[index].BuildMesh3D(main, hight);
                    }
                    else
                    {
                        squaresMap[index].BuildMesh2D(main);
                    }
                }
            }

        }
        else
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    int index = i * y + j;
                    squaresMap[index] = new MarchingSquare(new Vector3(i * size + halfSize, 0, j * size + halfSize), threshold, halfSize, islerp, offset, scale);
                    if (Build3D)
                    {
                        squaresMap[index].BuildMesh3D(main, hight);
                    }
                    else
                    {
                        squaresMap[index].BuildMesh2D(main);
                    }
                }
            }
        }
        main.FillArray();
        mesh.vertices = main.vers;
        mesh.triangles = main.tris;
        mesh.colors = main.color;
        mesh.RecalculateNormals();
        ConfigCollider();

    }
    protected void OnDrawGizmos()
    {

        if (squaresMap != null)
        {
            for (int i = 0; i < squaresMap.Length; i++)
            {
                squaresMap[i].DrawPoints(transform.position, debugSettings);
                squaresMap[i].DrawLines(transform.position, debugSettings);
            }
        }
    }
}
[Serializable]
public class MarchingSquaresDebugSettings
{
    [Header("Point")]
    public bool debugCenterPoint = true;
    public float centerPointSize = 0.2f;
    public Color centerPointColor = Color.white;
    public bool debugCornerPoint = true;
    public bool debugCornerPointAlpha = false;
    public float cornerPointSize = 0.2f;
    public Color cornerPointColor = Color.white;
    public bool debugMidPoint = true;
    public float midPointSize = 0.2f;
    public Color midPointColor = Color.white;

    [Header("Line")]
    public bool debugLine = true;
    public Color lineColor;
}
struct point
{
    public int value;
}
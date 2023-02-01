using UnityEngine;

public class MarchingCube
{
    public Vector3 center;
    //8顶点
    public Vector3[] corner;
    /// <summary>
    ///8个
    /// </summary>
    public float[] values;
    public MarchingCube(Vector3 cent, Vector3[] corn, float[] v)
    {
        center = cent;
        corner = corn;
        values = v;
    }

    public void BuildMesh(ProceduralMeshPart main, float threshold, bool lerp)
    {
        int index = 0;

        for (int i = 0; i < 8; i++)
        {
            if (values[i] < threshold)
            {
                index |= 1 << i;
            }
        }
        //这里的index所对应的是位掩码得到的最终结果，循环内写的则是构建面，因为面一定是三个一组，所以3为循环，表中则是3个一组的来写边。而我们可以通过对应的边来找到对应的端点。
        for (int i = 0; MarchingCubesTable.triangulation[index][i] != -1; i += 3)
        {
            int a0 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[index][i]];
            int b0 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[index][i]];

            int a1 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[index][i + 1]];
            int b1 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[index][i + 1]];

            int a2 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[index][i + 2]];
            int b2 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[index][i + 2]];

            Vector3 A;
            Vector3 B;
            Vector3 C;
            if (lerp)
            {
                A = Vector3.Lerp(corner[a0], corner[b0], values[a0] / (values[a0] + values[b0]));
                B = Vector3.Lerp(corner[a1], corner[b1], values[a1] / (values[a1] + values[b1]));
                C = Vector3.Lerp(corner[a2], corner[b2], values[a2] / (values[a2] + values[b2]));
            }
            else
            {
                A = (corner[a0] + corner[b0]) * 0.5f;
                B = (corner[a1] + corner[b1]) * 0.5f;
                C = (corner[a2] + corner[b2]) * 0.5f;
            }
            main.AddTriangle(A, B, C);
        }
    }
}

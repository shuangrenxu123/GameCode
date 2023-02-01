using UnityEngine;

public class MarchingCube
{
    public Vector3 center;
    //8����
    public Vector3[] corner;
    /// <summary>
    ///8��
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
        //�����index����Ӧ����λ����õ������ս����ѭ����д�����ǹ����棬��Ϊ��һ��������һ�飬����3Ϊѭ������������3��һ�����д�ߡ������ǿ���ͨ����Ӧ�ı����ҵ���Ӧ�Ķ˵㡣
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

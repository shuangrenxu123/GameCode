using System.Collections.Generic;
using UnityEngine;

public class NoiseTerrainGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;

    public ComputeShader CS;

    [Header("Noise Setting")]
    public float scale = 1;
    public Vector3 offset = Vector3.zero;
    [Min(1)] public int octaves = 4;//���
    [Min(0)] public float roughness = 3;//�ֲڶ�
    [Min(0)] public float persistance = 0.4f;//����
    public float hightScale;

    protected List<ComputeBuffer> buffersToRelease;
    public ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointPerAxis, float cubesize, Vector3 worldCenter)
    {
        CS.SetVector("scaleAndOffset", new Vector4(scale, offset.x, offset.y, offset.z));
        CS.SetInt("octaves", octaves);
        CS.SetFloat("roughness", roughness);
        CS.SetFloat("persistance", persistance);
        CS.SetFloat("hightScale", hightScale);

        //�ܵ��߳���
        int numPoints = numPointPerAxis * numPointPerAxis * numPointPerAxis;
        int numThreadsPerAxis = Mathf.CeilToInt((float)numPointPerAxis / (float)threadGroupSize);

        CS.SetBuffer(0, "points", pointsBuffer);
        CS.SetInt("numPointsPerAxis", numPointPerAxis);
        CS.SetVector("center", worldCenter);
        CS.SetFloats("cubeSize", cubesize);

        CS.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        return pointsBuffer;
    }
}

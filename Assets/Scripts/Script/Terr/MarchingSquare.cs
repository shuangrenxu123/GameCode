using UnityEngine;

public class MarchingSquare
{
    //中心点
    public Vector3 centerPoint;
    //四条边上的点  0-LB,1-LT,2-RT,3-RB
    public MarchingSquaresPoint[] cornerPoints;
    //四条边上的中点  0-ML,1-MT,2-MR,3-MB
    public Vector3[] midPoints;
    //限定值
    public float threshold = 0.5f;

    int value = -1;
    public MarchingSquare(int v, Vector3 center, float halfSize)
    {
        midPoints = new Vector3[4];
        value = v;
        midPoints[0] = center + halfSize * new Vector3(-1, 0, 0);
        midPoints[1] = center + halfSize * new Vector3(0, 0, 1);
        midPoints[2] = center + halfSize * new Vector3(1, 0, 0);
        midPoints[3] = center + halfSize * new Vector3(0, 0, -1);

        cornerPoints = new MarchingSquaresPoint[4];
        cornerPoints[0] = new MarchingSquaresPoint(center + halfSize * new Vector3(-1, 0, -1), -2);
        cornerPoints[1] = new MarchingSquaresPoint(center + halfSize * new Vector3(-1, 0, 1), -2);
        cornerPoints[2] = new MarchingSquaresPoint(center + halfSize * new Vector3(1, 0, 1), -2);
        cornerPoints[3] = new MarchingSquaresPoint(center + halfSize * new Vector3(1, 0, -1), -2);
    }
    public MarchingSquare(Vector3 center, float threshold, float halfSize, bool islerp, int offset, float s)
    {
        centerPoint = center;
        this.threshold = threshold;
        cornerPoints = new MarchingSquaresPoint[4];
        //矩形四个顶点
        Vector3 lbPos = center + halfSize * new Vector3(-1, 0, -1);
        Vector3 ltPos = center + halfSize * new Vector3(-1, 0, 1);
        Vector3 rtPos = center + halfSize * new Vector3(1, 0, 1);
        Vector3 rbPos = center + halfSize * new Vector3(1, 0, -1);
        cornerPoints[0] = new MarchingSquaresPoint(lbPos, 1 - Mathf.PerlinNoise(lbPos.x * s + offset, lbPos.z + offset));
        cornerPoints[1] = new MarchingSquaresPoint(ltPos, 1 - Mathf.PerlinNoise(ltPos.x * s + offset, ltPos.z + offset));
        cornerPoints[2] = new MarchingSquaresPoint(rtPos, 1 - Mathf.PerlinNoise(rtPos.x * s + offset, rtPos.z + offset));
        cornerPoints[3] = new MarchingSquaresPoint(rbPos, 1 - Mathf.PerlinNoise(rbPos.x * s + offset, rbPos.z + offset));

        midPoints = new Vector3[4];

        if (islerp)
        {
            float lbTolt = cornerPoints[0].value / (cornerPoints[0].value + cornerPoints[1].value);
            midPoints[0] = Vector3.Lerp(ltPos, lbPos, lbTolt);
            float ltTort = cornerPoints[1].value / (cornerPoints[1].value + cornerPoints[2].value);
            midPoints[1] = Vector3.Lerp(rtPos, ltPos, ltTort);
            float rtTorb = cornerPoints[2].value / (cornerPoints[2].value + cornerPoints[3].value);
            midPoints[2] = Vector3.Lerp(rbPos, rtPos, rtTorb);
            float rbTolb = cornerPoints[3].value / (cornerPoints[3].value + cornerPoints[0].value);
            midPoints[3] = Vector3.Lerp(lbPos, rbPos, rbTolb);
        }
        else
        {
            midPoints[0] = center + halfSize * new Vector3(-1, 0, 0);
            midPoints[1] = center + halfSize * new Vector3(0, 0, 1);
            midPoints[2] = center + halfSize * new Vector3(1, 0, 0);
            midPoints[3] = center + halfSize * new Vector3(0, 0, -1);
        }
    }

    private int GetBitMaskValue()
    {
        int lbValue = cornerPoints[0].value > threshold ? 1 : 0;
        int ltValue = cornerPoints[1].value > threshold ? 2 : 0;
        int rtValue = cornerPoints[2].value > threshold ? 4 : 0;
        int rbValue = cornerPoints[3].value > threshold ? 8 : 0;
        return lbValue + ltValue + rtValue + rbValue;
    }

    public void DrawLines(Vector3 worldCenter, MarchingSquaresDebugSettings debugSettings)
    {
        if (debugSettings.debugLine)
        {
            int bitMask = GetBitMaskValue();
            Gizmos.color = debugSettings.lineColor;
            switch (bitMask)
            {
                case 0:
                    break;
                case 1:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[3]);
                    break;
                case 2:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[1]);
                    break;
                case 3:
                    Gizmos.DrawLine(worldCenter + midPoints[1], worldCenter + midPoints[3]);
                    break;
                case 4:
                    Gizmos.DrawLine(worldCenter + midPoints[1], worldCenter + midPoints[2]);
                    break;
                case 5:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[1]);
                    Gizmos.DrawLine(worldCenter + midPoints[2], worldCenter + midPoints[3]);
                    break;
                case 6:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[2]);
                    break;
                case 7:
                    Gizmos.DrawLine(worldCenter + midPoints[2], worldCenter + midPoints[3]);
                    break;
                case 8:
                    Gizmos.DrawLine(worldCenter + midPoints[2], worldCenter + midPoints[3]);
                    break;
                case 9:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[2]);
                    break;
                case 10:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[3]);
                    Gizmos.DrawLine(worldCenter + midPoints[1], worldCenter + midPoints[2]);
                    break;
                case 11:
                    Gizmos.DrawLine(worldCenter + midPoints[1], worldCenter + midPoints[2]);
                    break;
                case 12:
                    Gizmos.DrawLine(worldCenter + midPoints[1], worldCenter + midPoints[3]);
                    break;
                case 13:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[1]);
                    break;
                case 14:
                    Gizmos.DrawLine(worldCenter + midPoints[0], worldCenter + midPoints[3]);
                    break;
                case 15:
                    break;
            }
        }
    }
    public void DrawPoints(Vector3 worldCenter, MarchingSquaresDebugSettings debugSettings)
    {
        if (debugSettings.debugCenterPoint)
        {
            Gizmos.color = debugSettings.centerPointColor;
            Gizmos.DrawSphere(centerPoint + worldCenter, debugSettings.centerPointSize);
        }
        if (debugSettings.debugCornerPoint)
        {
            for (int i = 0; i < cornerPoints.Length; i++)
            {
                if (cornerPoints[i].value > threshold)
                {
                    if (debugSettings.debugCornerPointAlpha)
                    {
                        Gizmos.color = debugSettings.cornerPointColor * cornerPoints[i].value;
                    }
                    else
                    {
                        Gizmos.color = debugSettings.cornerPointColor;
                    }
                    Gizmos.DrawSphere(cornerPoints[i].pos + worldCenter, debugSettings.cornerPointSize);
                }
            }
        }
        if (debugSettings.debugMidPoint)
        {
            Gizmos.color = debugSettings.midPointColor;
            for (int i = 0; i < midPoints.Length; i++)
            {
                Gizmos.DrawSphere(midPoints[i] + worldCenter, debugSettings.midPointSize);
            }
        }
    }

    public void BuildMesh2D(ProceduralMeshPart meshPart)
    {
        if (value == -1)
        {
            value = GetBitMaskValue();
        }
        switch (value)
        {
            case 0:
                break;
            case 1:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3]);
                break;
            case 2:
                meshPart.AddTriangle(cornerPoints[1].pos, midPoints[1], midPoints[0]);
                break;
            case 3:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[3]);
                break;
            case 4:
                meshPart.AddTriangle(cornerPoints[2].pos, midPoints[2], midPoints[1]);
                break;
            case 5:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3]);
                meshPart.AddQuad(midPoints[3], midPoints[0], midPoints[1], midPoints[2]);
                meshPart.AddTriangle(midPoints[1], cornerPoints[2].pos, midPoints[2]);
                break;
            case 6:
                meshPart.AddQuad(midPoints[0], cornerPoints[1].pos, cornerPoints[2].pos, midPoints[2]);
                break;
            case 7:
                meshPart.AddPentagon(cornerPoints[2].pos, midPoints[2], midPoints[3], cornerPoints[0].pos, cornerPoints[1].pos);
                break;
            case 8:
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3]);
                break;
            case 9:
                meshPart.AddQuad(cornerPoints[0].pos, midPoints[0], midPoints[2], cornerPoints[3].pos);
                break;
            case 10:
                meshPart.AddTriangle(midPoints[0], cornerPoints[1].pos, midPoints[1]);
                meshPart.AddQuad(midPoints[0], midPoints[1], midPoints[2], midPoints[3]);
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3]);
                break;
            case 11:
                meshPart.AddPentagon(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[2], cornerPoints[3].pos);
                break;
            case 12:
                meshPart.AddQuad(midPoints[3], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos);
                break;
            case 13:
                meshPart.AddPentagon(cornerPoints[0].pos, midPoints[0], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos);
                break;
            case 14:
                meshPart.AddPentagon(cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos, midPoints[3], midPoints[0]);
                break;
            case 15:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos);
                break;
            default:
                Debug.LogError("bit error");
                break;
        }
    }
    public void BuildMesh3D(ProceduralMeshPart meshPart, int height)
    {
        if (value == -1)
        {
            value = GetBitMaskValue();
        }
        switch (value)
        {
            case 0:
                break;
            case 1:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3], height);
                break;
            case 2:
                meshPart.AddTriangle(cornerPoints[1].pos, midPoints[1], midPoints[0], height);
                break;
            case 3:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[3], height);
                break;
            case 4:
                meshPart.AddTriangle(cornerPoints[2].pos, midPoints[2], midPoints[1], height);
                break;
            case 5:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3], height);
                meshPart.AddQuad(midPoints[3], midPoints[0], midPoints[1], midPoints[2], height);
                meshPart.AddTriangle(midPoints[1], cornerPoints[2].pos, midPoints[2], height);
                break;
            case 6:
                meshPart.AddQuad(midPoints[0], cornerPoints[1].pos, cornerPoints[2].pos, midPoints[2], height);
                break;
            case 7:
                meshPart.AddPentagon(cornerPoints[2].pos, midPoints[2], midPoints[3], cornerPoints[0].pos, cornerPoints[1].pos, height);
                break;
            case 8:
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3], height);
                break;
            case 9:
                meshPart.AddQuad(cornerPoints[0].pos, midPoints[0], midPoints[2], cornerPoints[3].pos, height);
                break;
            case 10:
                meshPart.AddTriangle(midPoints[0], cornerPoints[1].pos, midPoints[1], height);
                meshPart.AddQuad(midPoints[0], midPoints[1], midPoints[2], midPoints[3], height);
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3], height);
                break;
            case 11:
                meshPart.AddPentagon(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[2], cornerPoints[3].pos, height);
                break;
            case 12:
                meshPart.AddQuad(midPoints[3], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos, height);
                break;
            case 13:
                meshPart.AddPentagon(cornerPoints[0].pos, midPoints[0], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos, height);
                break;
            case 14:
                meshPart.AddPentagon(cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos, midPoints[3], midPoints[0], height);
                break;
            case 15:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos, height);
                break;
            default:
                Debug.LogError("bit error");
                break;
        }
    }
}
public class MarchingSquaresPoint
{
    public Vector3 pos;
    public float value;

    public MarchingSquaresPoint(Vector3 pos, float value)
    {
        this.pos = pos;
        this.value = value;
    }
}
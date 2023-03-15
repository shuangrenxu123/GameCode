using PlayerInfo;
using UnityEngine;

public class NetWorkUtility
{
    public static PlayerInfo.vector3 ToProtoBufV3(Vector3 v3)
    {
        vector3 value = new vector3();
        value.X = v3.x;
        value.Y = v3.y;
        value.Z = v3.z;
        return value;
    }
    public static Vector3 ToUnityV3(vector3 v3)
    {
        Vector3 value = new Vector3();
        value.x = v3.X;
        value.y = v3.Y;
        value.z = v3.Z;
        return value;
    }
}

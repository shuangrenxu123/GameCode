using System;
using UnityEngine;

[Serializable]
public struct CharacterMountPoint
{
    [Tooltip("挂点对应的装备部位")]
    public EquipType part;

    [Tooltip("该部位对应的 Transform 节点")]
    public Transform point;
}

public class CharacterMountPoints : MonoBehaviour
{
    [SerializeField]
    private CharacterMountPoint[] mountPoints = Array.Empty<CharacterMountPoint>();

    public Transform GetMountPoint(EquipType part)
    {
        TryGetMountPoint(part, out Transform point);
        return point;
    }

    public bool TryGetMountPoint(EquipType part, out Transform point)
    {
        for (int i = 0; i < mountPoints.Length; i++)
        {
            CharacterMountPoint mountPoint = mountPoints[i];
            if (mountPoint.part != part)
            {
                continue;
            }

            point = mountPoint.point;
            return point != null;
        }

        point = null;
        return false;
    }
}

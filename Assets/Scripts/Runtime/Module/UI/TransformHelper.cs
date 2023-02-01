using UnityEngine;

///<summary>
///变换组件助手类
///</summary>
public static class TransformHelper
{
    /// <summary>
    /// 未知层级查找指定名称的子物体变换组件
    /// </summary>
    /// <param name="CurrentTransfrom">当前变换组件</param>
    /// <param name="childName">要查找的子物体名称</param>
    /// <returns>返回变换组件</returns>
    public static Transform FindChildByName(this Transform CurrentTransfrom, string childName)
    {
        //递归：方法内部又调用自身
        Transform childTransform = CurrentTransfrom.Find(childName);
        if (childTransform != null) return childTransform;
        for (int i = 0; i < CurrentTransfrom.childCount; i++)
        {
            childTransform = FindChildByName(CurrentTransfrom.GetChild(i), childName);
            if (childTransform != null) return childTransform;
        }
        return null;
    }
}

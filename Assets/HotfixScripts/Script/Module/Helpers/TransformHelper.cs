using UnityEngine;

///<summary>
///变换组件助手类
///</summary>
public static class TransformHelper
{
    /// <summary>
    /// 未知层级查找指定名称的子物体变换组件
    /// </summary>
    /// <param name="CurrentTransform">当前变换组件</param>
    /// <param name="childName">要查找的子物体名称</param>
    /// <returns>返回变换组件</returns>
    public static Transform FindChildByName(this Transform CurrentTransform, string childName)
    {
        Transform childTransform = CurrentTransform.Find(childName);
        if (childTransform != null) return childTransform;
        for (int i = 0; i < CurrentTransform.childCount; i++)
        {
            childTransform = FindChildByName(CurrentTransform.GetChild(i), childName);
            if (childTransform != null) return childTransform;
        }
        return null;
    }
    public static void RemoveAllChildren(this Transform parent)
    {
        Transform transform;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            transform = parent.transform.GetChild(i);
            GameObject.Destroy(transform.gameObject);
        }
    }
    public static void HideAllChildren(this Transform parent)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            parent.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public static void ShowAllChildren(this Transform parent)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            parent.transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}

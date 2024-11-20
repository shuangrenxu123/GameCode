using UnityEngine;

public static class ComponentHelper
{
    public static T AddOrGetComponent<T>(this Transform transform) where T : Component
    {
        var component = transform.GetComponent<T>();
        if (component == null)
        {
            component = transform.gameObject.AddComponent<T>();
        }

        return component;
    }

}

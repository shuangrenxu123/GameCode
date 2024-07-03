using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T :class
{
    private static T _instance;
    public static T Instance => _instance;
    private void Awake()
    {
        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}

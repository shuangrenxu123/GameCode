using UnityEngine;

[System.Serializable]
public class Volume
{
    public string tagName;
    [Header("Movement")]
    [Min(0.01f)]
    public float accelerationMultiplier = 1f;
    [Min(0.01f)]
    public float decelerationMultiplier = 1f;
    [Min(0.01f)]
    public float speedMultiplier = 1f;
    [Range(0.05f, 50f)]
    public float gravityAscendingMultiplier = 1f;
    [Range(0.05f, 50f)]
    public float gravityDescendingMultiplier = 1f;
}
[System.Serializable]
public class Surface
{
    public string tagName = "";

    [Header("Movement")]

    [Min(0.01f)]
    public float accelerationMultiplier = 1f;

    [Min(0.01f)]
    public float decelerationMultiplier = 1f;

    [Min(0.01f)]
    public float speedMultiplier = 1f;
}

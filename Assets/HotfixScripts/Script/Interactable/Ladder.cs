using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField]
    public Transform topReference = null;

    [SerializeField]
    public Transform bottomReference = null;

    [Header("Properties")]

    [Min(0)]
    public int climbCount = 1;


}


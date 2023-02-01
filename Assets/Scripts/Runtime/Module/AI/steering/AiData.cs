using System.Collections.Generic;
using UnityEngine;

public class AiData : MonoBehaviour
{
    [SerializeField]
    public CombatNumberBox CombatNumberBox;
    [HideInInspector]
    /// <summary>
    /// 目标数组
    /// </summary>
    public List<Transform> targets = null;
    [HideInInspector]
    /// <summary>
    /// 障碍物数组
    /// </summary>
    public Collider2D[] obstacles = null;
    [HideInInspector]
    /// <summary>
    /// 是否得到随即目标
    /// </summary>
    public bool reachedLastTarget = true;
    [HideInInspector]
    /// <summary>
    /// 当前应该到达的目标点
    /// </summary>
    public Transform currentTarget;
    [HideInInspector]
    /// <summary>
    /// 敌人目标
    /// </summary>
    public Transform enemy;
    [HideInInspector]
    /// <summary>
    /// 自己的坐标
    /// </summary>
    public Transform me;

    private void Awake()
    {
        CombatNumberBox = new CombatNumberBox();
        CombatNumberBox.Init();
    }
    private void Start()
    {
        me = gameObject.transform;
    }
    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}

using System.Collections.Generic;
using UnityEngine;

public class Detector
{
    AiData data;
    /// <summary>
    /// 目标范围距离
    /// </summary>
    [SerializeField]
    private float targetDetectionRange;
    /// <summary>
    /// 障碍物的检测范围
    /// </summary>
    [SerializeField]
    private float obstaclesRange = 2;
    /// <summary>
    /// 障碍物图层和玩家图层
    /// </summary>
    [SerializeField]
    private LayerMask obstaclesLayerMask, PlayerMask;
    /// <summary>
    /// 目标合集
    /// </summary>
    private List<Transform> targets;
    /// <summary>
    /// 障碍物合集
    /// </summary>
    private Collider2D[] colliders;
    public Detector(AiData aiData)
    {
        data = aiData;
        obstaclesLayerMask = LayerMask.NameToLayer("obstacles");
        PlayerMask = LayerMask.NameToLayer("Player");
        targetDetectionRange = 1f;
    }
    public void Find()
    {
        FindTargets();
        FindObstacles();
    }
    private void FindObstacles()
    {
        colliders = Physics2D.OverlapCircleAll(data.me.position, obstaclesRange);
        data.obstacles = colliders;
    }

    private void FindTargets()
    {
        Collider2D[] playerCollider = Physics2D.OverlapCircleAll(data.me.position, targetDetectionRange, 1 << PlayerMask);
        List<Transform> temp = new List<Transform>();
        if (playerCollider.Length != 0)
        {
            foreach (var collider in playerCollider)
            {
                //指向玩家的射线  
                Vector2 direction = collider.transform.position - data.me.position;
                RaycastHit2D hit = Physics2D.Raycast(data.me.position, direction, direction.magnitude, 1 << obstaclesLayerMask);
                if (hit.collider == null)
                {
                    temp.Add(collider.transform);
                }
            }
            targets = new List<Transform>(temp);
        }
        if (temp.Count == 0)
            targets = null;
        else
            targets = temp;
        //Debug.Log("周围没有目标");
        data.targets = targets;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Detector
{
    AiData data;
    /// <summary>
    /// Ŀ�귶Χ����
    /// </summary>
    [SerializeField]
    private float targetDetectionRange;
    /// <summary>
    /// �ϰ���ļ�ⷶΧ
    /// </summary>
    [SerializeField]
    private float obstaclesRange = 2;
    /// <summary>
    /// �ϰ���ͼ������ͼ��
    /// </summary>
    [SerializeField]
    private LayerMask obstaclesLayerMask, PlayerMask;
    /// <summary>
    /// Ŀ��ϼ�
    /// </summary>
    private List<Transform> targets;
    /// <summary>
    /// �ϰ���ϼ�
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
                //ָ����ҵ�����  
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
        //Debug.Log("��Χû��Ŀ��");
        data.targets = targets;
    }
}

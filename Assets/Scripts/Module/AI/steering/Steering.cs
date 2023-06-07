using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Steering
{
    AiData aiData;
    private Vector2 targetPositionCached;
    private float agentColliderSize = 0.5f;
    private float radius = 2f;
    GameObject t;
    public Steering(AiData aiData)
    {
        this.aiData = aiData;
        t = new GameObject("NullTarget");
        t.transform.position = Vector3.zero;
    }
    public void GetSteering(ref float[] interest, ref float[] danger)
    {
        GetTargetSteering(ref interest);
        GetObstacleSteering(ref danger);
    }
    private void GetTargetSteering(ref float[] interest)
    {
        if (aiData.reachedLastTarget)
        {
            if (aiData.targets == null || aiData.targets.Count <= 0)
            {
                while (true)
                {
                    Vector2 Nextpos = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) + (Vector2)aiData.me.position;
                    var direction = Nextpos - (Vector2)aiData.me.position;
                    t.transform.position = Nextpos;
                    RaycastHit2D hit = Physics2D.Raycast(aiData.me.position, direction, direction.magnitude, 1 << LayerMask.NameToLayer("obstacles"));
                    if (hit.collider == null)
                    {
                        targetPositionCached = t.transform.position;
                        aiData.reachedLastTarget = false;
                        aiData.currentTarget = t.transform;
                        return;
                    }
                }
            }
        }
        if (aiData.targets != null)
        {
            aiData.enemy = aiData.targets.OrderBy
                (target => Vector2.Distance(target.position, aiData.me.position)).FirstOrDefault();
            t.transform.position = aiData.enemy.position;
            targetPositionCached = t.transform.position;
        }
        else
        {
            aiData.enemy = null;
        }
        aiData.currentTarget = t.transform;
        Vector2 directionToTarget = (targetPositionCached - (Vector2)aiData.me.position);
        for (int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(directionToTarget.normalized, Directions.eightDirections[i]);
            if (result > 0)
            {
                float valuePutIn = result;
                if (valuePutIn > interest[i])
                {
                    interest[i] = valuePutIn;
                }
            }
        }
        return;
    }
    private void GetObstacleSteering(ref float[] danger)
    {
        foreach (Collider2D collider2D in aiData.obstacles)
        {
            Vector2 directionToObstacle = collider2D.ClosestPoint(aiData.me.position) - (Vector2)aiData.me.position;
            float distance = directionToObstacle.magnitude;

            float weight = distance <= agentColliderSize ? 1 : (radius - distance) / radius;

            Vector2 directionNormalize = directionToObstacle.normalized;

            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector2.Dot(directionNormalize, Directions.eightDirections[i]);
                float value = result * weight;
                if (value > danger[i])
                {
                    danger[i] = value;
                }
            }
        }
    }
}
public static class Directions
{
    public static List<Vector2> eightDirections = new List<Vector2>{
            new Vector2(0,1).normalized,
            new Vector2(1,1).normalized,
            new Vector2(1,0).normalized,
            new Vector2(1,-1).normalized,
            new Vector2(0,-1).normalized,
            new Vector2(-1,-1).normalized,
            new Vector2(-1,0).normalized,
            new Vector2(-1,1).normalized
        };
}

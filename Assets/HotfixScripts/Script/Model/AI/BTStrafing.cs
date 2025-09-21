using BT;
using UnityEngine;
using Utility;

public class BTStrafing<TKey, TValue> : BTAction<TKey, TValue>
{
    private Enemy enemy;
    private float verticalMovementValue = 0;
    private float horizontalMovementValue = 0;
    private float housingDistanceSql = 400;
    private float insideDistanceSql = 100;
    private float timer;
    private float time = 2f;
    public BTStrafing(Enemy enemy)
    {
        this.enemy = enemy;
    }
    protected override void Enter()
    {
        verticalMovementValue = Random.Range(-0.5f, 1);
        horizontalMovementValue = Random.Range(-1f, 1f);
    }
    protected override BTResult Execute()
    {
        timer += Time.deltaTime;
        bool attack = false;
        if (timer > time)
        {
            attack = Probability.GetBool(70);
            timer = 0;
        }
        var target = database.GetData<Transform>((dynamic)"targetTransform");
        Vector3 lookat = target.position - enemy.transform.position;
        lookat.y = 0;
        enemy.transform.rotation = Quaternion.LookRotation(lookat);
        //到敌人距离的平方
        float distanceSqr = (enemy.transform.position - target.position).sqrMagnitude;
        //if(distanceSqr <= 4)
        //{
        //    bool b = Probability.GetBool(10);
        //    if(b) 
        //    {
        //        anim.PlayTargetAnimation("step",true);
        //    }
        //    return BTResult.Success;
        //}
        if (attack)
        {
            return BTResult.Success;
        }
        return BTResult.Running;
    }
}

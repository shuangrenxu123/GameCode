using System;
using Fight;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{

    public CombatEntity combatEntity;
    //public SkillSystem skillSystem;
    //public EnemyAI ai;
    public bool isHit;
    public bool isDead = false;
    private void Awake()
    {
        combatEntity = GetComponent<CombatEntity>();
        //skillSystem = new SkillSystem(this);
        //skillSystem.AddSkill(Resources.Load<SkillData>("fire"), Resources.Load<GameObject>("jili"));

    }

    private void Start()
    {
        combatEntity.Init(100);
        //ai = new EnemyAI();
        //ai.Init(this, animatorHandle as EnemyAnimatorHandle, this);
    }
    private void Update()
    {
        if (isDead)
        {
            Dead();
        }
        else
        {
            //ai.Update();
        }
    }
    private void Dead()
    {

    }
}

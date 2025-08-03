using System;
using Fight;
using UnityEngine;
using UnityEngine.Events;
using static Fight.Number.CombatNumberBox;

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
        combatEntity.hp.SetMaxValue(100);

        combatEntity.properties.RegisterAttribute(PropertyType.Attack, 10);
        combatEntity.properties.RegisterAttribute(PropertyType.Defense, 10);
        combatEntity.properties.RegisterAttribute(PropertyType.SpeedMultiplier, 10);

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

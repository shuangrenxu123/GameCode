using Fight;
using UnityEngine;

public class Enemy : CharacterManager
{
    public CombatEntity combatEntity;
    public SkillSystem skillSystem;
    public EnemyAI ai;
    public bool isHit;
    public bool isDead = false;
    private void Awake()
    {
        combatEntity = GetComponent<CombatEntity>();
        skillSystem = new SkillSystem(this);
        skillSystem.AddSkill(Resources.Load<SkillData>("fire"), Resources.Load<GameObject>("jili"));
        animatorHandle = GetComponentInChildren<EnemyAnimatorHandle>();
        characterController = GetComponent<CharacterController>();

    }
    private void Start()
    {
        combatEntity.Init(100);
        ai = new EnemyAI();
        ai.Init(this, animatorHandle as EnemyAnimatorHandle, this);
    }
    private void Update()
    {
        if (isDead)
        {
            Dead();
        }
        else
        {
            ai.Update();
        }
    }
    private void Dead()
    {

    }
}

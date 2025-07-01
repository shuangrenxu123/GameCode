using System.Collections.Generic;
using Fight;
using Skill;
using UnityEngine;

public class CustomBuffSkill : MonoBehaviour
{
    SkillSystem system;
    /// <summary>
    /// ��Чʱ��
    /// </summary>
    [SerializeField]
    float time;
    /// <summary>
    /// �����������
    /// </summary>
    [SerializeField]
    float DestroyTime;
    [SerializeField]
    int value;
    [SerializeField]
    SkillEffect type;

    float timer;
    SkillData skillData;
    private void Start()
    {
        timer = 0;
    }
    public void Init(SkillSystem system, SkillData data)
    {
        skillData = data;
        this.system = system;
    }
    private void Update()
    {
        if (timer >= time)
        {
            Execute();
            time += DestroyTime;
        }
        if (timer >= DestroyTime)
        {
            Destroy();
        }
        timer += Time.deltaTime;
    }
    void Execute()
    {
        List<CombatEntity> target = new();
        switch (skillData.AffectTargetType)
        {
            case SkillAffectTargetType.Self:
                target.Add(system.CombatEntity);
                break;
            case SkillAffectTargetType.SelfTeam:
                break;
            case SkillAffectTargetType.EnemyTeam:
                break;
            default:
                break;
        }
        switch (type)
        {
            case SkillEffect.Damage:
                break;
            case SkillEffect.HP:
                CombatActionFactor.CreateActionAndExecute<RegenerationAction>
                (system.CombatEntity, target, value);
                break;
            case SkillEffect.Attack:
                break;
            default:
                break;
        }
    }

    void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}

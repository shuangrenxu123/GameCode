using Fight;
using Skill;
using System.Collections.Generic;
using UnityEngine;

public class CustomBuffSkill : MonoBehaviour
{
     SkillSystem system;
    /// <summary>
    /// 生效时间
    /// </summary>
    [SerializeField]
    float time; 
    /// <summary>
    /// 多少秒后销毁
    /// </summary>
    [SerializeField]
    float DestoryTime;
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
    public void Init(SkillSystem system,SkillData data)
    {
        skillData = data;
        this.system = system;
    }
    private void Update()
    {
        if(timer >= time)
        {
            Excute();
            time += DestoryTime;
        }
        if (timer >= DestoryTime)
        {
            Destroy();
        }
        timer += Time.deltaTime;
    }
    void Excute()
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
                new RegenerationAction(system.CombatEntity, target).Apply(value);
                Debug.Log($"恢复了生命值{value}");
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

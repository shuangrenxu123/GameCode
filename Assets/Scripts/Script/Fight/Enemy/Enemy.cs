using Fight;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : CharacterManager
{
    public CombatEntity entity;
    public SkillSystem skillSystem;
    private void Awake()
    {
        entity = GetComponent<CombatEntity>();
        skillSystem = new SkillSystem();
        skillSystem.AddSkill(Resources.Load<SkillData>("fire"),Resources.Load<GameObject>("jili"));
    }
    private void Start()
    {
        entity.Init(100);
    }
}

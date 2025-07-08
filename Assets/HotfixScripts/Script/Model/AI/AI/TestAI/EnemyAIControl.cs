using Animancer;
using Fight;
using Skill;
using UnityEngine;

public class EnemyAIControl : MonoBehaviour
{
    EnemyAI Ai;
    [SerializeField]
    CharacterActor actor;
    [SerializeField]
    CombatEntity entity;
    public SkillSystem skillSystem;
    public DataBase dataBase { get; private set; }
    public CCAnimatorConfig config;
    public SkillRunner skillRunner;
    public AnimancerComponent anim;

    private void Start()
    {
        entity = GetComponent<CombatEntity>();
        skillSystem = new(entity);

        skillSystem.AddSkill(Resources.Load<SkillData>("skill/buffData"), Resources.Load<GameObject>("skill/buff"));

        Ai = new EnemyAI(this);
        dataBase = new();

        Ai.actor = actor;
        Ai.Init(null, dataBase);

    }
    private void Update()
    {
        Ai.Update();
    }
}

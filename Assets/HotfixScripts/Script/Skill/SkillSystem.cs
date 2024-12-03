using Skill;
using System.Collections.Generic;
using UnityEngine;
namespace Fight
{
    /// <summary>
    /// ������Ҫ���Ƽ��ܵ��������������Ҫ���ڻ�����µ�GameObject�����
    /// </summary>
    public class SkillSystem
    {
        Transform transform;
        public CombatEntity CombatEntity { get; private set; }

        public SkillSystem(CombatEntity entity)
        {
            this.CombatEntity = entity;
            this.transform = CombatEntity.transform;
        }
        private Dictionary<string, SkillAbility> skills = new Dictionary<string, SkillAbility>();
        public void AddSkill(SkillData data, GameObject prefab)
        {
            skills.Add(data.skillName, new SkillAbility(data, prefab));
        }
        public GameObject GenerateSkill(string name, Vector3 position)
        {
            var go = skills[name].CreateExecution(position);
            go.GetComponent<CustomBuffSkill>().Init(this, skills[name].data);
            return go;
        }
        public GameObject GenerateSkill(string name)
        {
            return GenerateSkill(name, transform.position);
        }
    }
}
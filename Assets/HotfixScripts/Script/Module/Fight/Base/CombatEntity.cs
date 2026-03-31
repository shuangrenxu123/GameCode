using System;
using Fight.Number;
using UnityEngine;
using UnityEngine.Events;

namespace Fight
{
    public enum CombatEntityType
    {
        Player,
        Enemy,
        ally
    }
    public sealed class CombatEntity : MonoBehaviour
    {
        [SerializeField]
        CombatEntityType entityType;

        public ResourceValue hp;
        public CombatPropertySet properties;
        ActionPointManager ActionPointManager;
        BuffManager buffManager;
        public UnityEvent onEntityDead = new UnityEvent();

        public void Awake()
        {
            ActionPointManager = new ActionPointManager();
            ActionPointManager.Init();

            properties = new CombatPropertySet();
            hp = properties.RegisterResource(ResourceType.Hp, PropertyType.MaxHp);
            hp.OnEmpty += OnEntityDead;
            buffManager = new BuffManager(this);
        }
        private void Update()
        {
            buffManager.OnUpdate();
        }

        void OnEntityDead()
        {
            onEntityDead.Invoke();
        }
        public void AddListener(ActionPointType actionPointType, Action<CombatAction> action)
        {
            ActionPointManager.AddListener(actionPointType, action);
        }

        public void RemoveListener(ActionPointType actionPointType, Action<CombatAction> action)
        {
            ActionPointManager.RemoveListener(actionPointType, action);
        }

        public void TriggerActionPoint(ActionPointType actionPointType, CombatAction action)
        {
            ActionPointManager.TriggerActionPoint(actionPointType, action);
        }
    }

}

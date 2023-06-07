
using System;
using UnityEngine;

namespace Fight
{
    public class CombatEntity : MonoBehaviour
    {
        public StateUI stateUI;
        public HealthPoint hp;
        public ActionPointManager ActionPointManager;
        public CombatNumberBox numberBox;
        public AnimatorManager animator;
        public BuffManager buffManager;
        public virtual void Awake()
        {
            hp = new HealthPoint();
            ActionPointManager = new ActionPointManager();
            numberBox = new CombatNumberBox();
            buffManager = new BuffManager(this);
            animator = transform.GetChild(0).GetComponent<AnimatorManager>();
            ActionPointManager.Init();

        }
        private void Update()
        {
            buffManager.OnUpdate();
        }
        /// <summary>
        /// 由Entity在start里面调用,用于大部分属性的初始化
        /// </summary>
        /// <param name="h"></param>
        public virtual void Init(int h)
        {

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

        public virtual void TakeDamage(int damage,string animatorName)
        {

        }
    }
}
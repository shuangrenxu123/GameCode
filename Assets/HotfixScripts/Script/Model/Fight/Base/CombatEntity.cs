using System;
using Fight.Number;
using UnityEngine;

namespace Fight
{

    public enum CombatEntityType
    {
        Player,
        Enemy,
        ally
    }
    public class CombatEntity : MonoBehaviour
    {
        [SerializeField]
        CombatEntityType entityType;
        public HealthPoint hp;
        public ActionPointManager ActionPointManager;
        public CombatNumberBox numberBox;
        public BuffManager buffManager;
        public virtual void Awake()
        {
            hp = new HealthPoint();

            ActionPointManager = new ActionPointManager();
            ActionPointManager.Init();

            numberBox = new CombatNumberBox();
            numberBox.Init();

            buffManager = new BuffManager(this);

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
        /// <summary>
        /// 受伤
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="animatorName"></param>
        public virtual void TakeDamageFx(float dir)
        {
            ChooseWhichDirectionDamageCameFrom(dir);
        }
        protected virtual void ChooseWhichDirectionDamageCameFrom(float direction)
        {


        }
    }

}


using System;
using UnityEngine;

namespace Fight
{
    public class CombatEntity : MonoBehaviour
    {
        public HealthPoint hp;
        public ActionPointManager ActionPointManager;
        public CombatNumberBox numberBox;
        public EnemyAnimatorHandler enemyAnimator;
        private void Start()
        {
            hp = new HealthPoint();
            ActionPointManager = new ActionPointManager();
            numberBox = new CombatNumberBox();
            enemyAnimator = GetComponentInChildren<EnemyAnimatorHandler>();
            numberBox.Init();
            numberBox.Speed.SetBase(5);
        }
        public void Init(int h)
        {
            hp.SetMaxValue(h);
            ActionPointManager.Init();
            hp.Reset();
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

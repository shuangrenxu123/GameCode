using Audio;
using System;
using UnityEngine;

namespace Fight
{
    public class CombatEntity : MonoBehaviour
    {
        [SerializeField]
        AudioClip hit;
        [SerializeField]
        private Animator Animator;
        public HealthPoint hp;
        public ActionPointManager ActionPointManager;
        public CombatNumberBox numberBox;
        public BuffManager buffManager;
        public virtual void Awake()
        {
            hp = new HealthPoint();
            ActionPointManager = new ActionPointManager();
            numberBox = new CombatNumberBox();
            buffManager = new BuffManager(this);
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
            string currentDamageAnimation = null;
            if (direction >= 145 && direction <= 180)
            {
                currentDamageAnimation = "Damage_Forward_01";
            }
            else if (direction <= -145 && direction >= -180)
            {
                currentDamageAnimation = "Damage_Forward_01";
            }
            else if (direction >= -45 && direction <= 45)
            {
                currentDamageAnimation = "Damage_Back_01";
            }
            else if (direction >= -144 && direction <= -45)
            {
                currentDamageAnimation = "Damage_Left_01";
            }
            else if (direction >= 45 && direction <= 144)
            {
                currentDamageAnimation = "Damage_Right_01";
            }
            Animator.Play(currentDamageAnimation);
            AudioManager.Instance.PlayAudio(hit,AudioLayer.Sound);
        }
    }
}

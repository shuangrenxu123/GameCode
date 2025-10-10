using System;
using Fight;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Character.UI
{
    public class CharacterStateUI : MonoBehaviour
    {
        [SerializeField]
        Image hpBar;

        [SerializeField]
        Image hpBufferBar;

        [SerializeField, ReadOnly]
        CombatEntity combatEntity;

        float hpLerpDuration = 0.4f;

        void OnEnable()
        {
            combatEntity = GetComponentInParent<CombatEntity>();
            combatEntity.hp.OnHpMinus += OnHpMinus;
            combatEntity.hp.OnHpAdd += OnHpAdd;
        }

        void OnDisable()
        {
            combatEntity.hp.OnHpMinus -= OnHpMinus;
            combatEntity.hp.OnHpAdd -= OnHpAdd;
        }

        private void OnHpAdd(int _, int __)
        {
            var percent = combatEntity.hp.Percent;

            hpBar.fillAmount = percent;

            LMotion.Create(hpBufferBar.fillAmount, percent, hpLerpDuration)
                .WithOnComplete(() => hpBufferBar.fillAmount = percent)
                .Bind(x => hpBufferBar.fillAmount = x);
        }

        private void OnHpMinus(int _, int __)
        {
            var percent = combatEntity.hp.Percent;

            hpBar.fillAmount = percent;

            LMotion.Create(hpBufferBar.fillAmount, percent, hpLerpDuration)
                .Bind(x => hpBufferBar.fillAmount = x);
        }
    }
}

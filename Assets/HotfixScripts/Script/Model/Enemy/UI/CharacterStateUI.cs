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
        [SerializeField, LabelText("血条")]
        Image hpBar;

        [SerializeField, LabelText("血条缓冲")]
        Image hpBufferBar;

        [SerializeField]
        CanvasGroup canvasGroup;

        [SerializeField, LabelText("存在时间")]
        float existTime;

        [SerializeField, LabelText("渐变时间")]
        float fadeTime;

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
            // LMotion.Create(0,1,fadeTime).
        }
    }
}

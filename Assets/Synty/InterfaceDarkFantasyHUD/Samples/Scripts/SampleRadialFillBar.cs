// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Synty.Interface.DarkFantasyHUD.Samples
{
    /// <summary>
    ///     A simple radial fill bar that fills and empties.
    /// </summary>
    public class SampleRadialFillBar : MonoBehaviour
    {
        [Header("References")]
        public Image image;
        public TMP_Text text;

        [Header("Parameters")]
        public float fillAmountFull = 1f;
        public float inSpeed = 5f;
        public float outSpeed = 5f;
        public float startDelay;
        public float inDelay = 2.5f;
        public float outDelay = 2.5f;
        public string labelText = "{0}%";

        public string LabelText => string.Format(labelText, (image.fillAmount / fillAmountFull * 100f).ToString("0"));

        private void Awake()
        {
            if (image == null)
            {
                image = GetComponentInChildren<Image>();
            }

            if (text == null)
            {
                text = GetComponentInChildren<TMP_Text>();
            }
        }

        private void Reset()
        {
            image = GetComponentInChildren<Image>();
            text = GetComponentInChildren<TMP_Text>();
        }

        private void Start()
        {
            StartCoroutine(C_TweenBackAndForth());
        }

        private IEnumerator C_TweenBackAndForth()
        {
            yield return new WaitForSeconds(startDelay);

            while (true)
            {
                yield return C_TweenFloat(0, 1, inSpeed);
                yield return new WaitForSeconds(outDelay);
                yield return C_TweenFloat(1, 0, outSpeed);
                yield return new WaitForSeconds(inDelay);
            }
        }

        private IEnumerator C_TweenFloat(float startValue, float endValue, float duration)
        {
            float time = 0f;
            while (time < 1f)
            {
                time += Time.deltaTime / duration;
                float currentValue = Mathf.Lerp(startValue, endValue, time);

                image.fillAmount = currentValue * fillAmountFull;
                text?.SetText(LabelText);
                yield return null;
            }
        }
    }
}

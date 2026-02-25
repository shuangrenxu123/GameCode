// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System.Collections;
using UnityEngine;

namespace Synty.Interface.DarkFantasyHUD.Samples
{
    /// <summary>
    ///     A simple animator that loops between two values.
    /// </summary>
    public class SampleLoopAnimator : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;

        [Header("Parameters")]
        public string parameterName = "Health";
        public float inSpeed = 5f;
        public float outSpeed = 5f;
        public float startDelay;
        public float inDelay = 2.5f;
        public float outDelay = 2.5f;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void Reset()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (animator == null)
            {
                return;
            }

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

                animator.SetFloat(parameterName, currentValue);
                yield return null;
            }
        }
    }
}

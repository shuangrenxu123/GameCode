// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Synty.Interface.DarkFantasyHUD.Samples
{
    /// <summary>
    ///     A simple countdown label that counts down from a set time.
    /// </summary>
    public class SampleCountdownLabel : MonoBehaviour
    {
        [Header("References")]
        public Animator myAnimator;
        public TMP_Text text;

        [Header("Parameters")]
        public float initialDelay = 0;
        public float countdownTime = 30;
        public float updateInterval = 0.1f;
        public float timeUpDuration = 2.5f;
        public string timerFormat = "F1";
        public UnityEvent onCountdownComplete;

        [Header("External Objects")]
        [UnityEngine.Serialization.FormerlySerializedAs("animator")]
        public Animator otherObjectAnimator;
        [UnityEngine.Serialization.FormerlySerializedAs("setAnimatorActive")]
        public bool setOtherObjectAnimatorActive = true;

        private float currentTime;

        private void Reset()
        {
            text = GetComponentInChildren<TMP_Text>();
            myAnimator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            BeginTimer(initialDelay);
        }

        private void BeginTimer(float delay)
        {
            currentTime = countdownTime;
            RefreshUI();

            StartCoroutine(C_TickDown(delay));
        }

        private IEnumerator C_TickDown(float delay)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            if (myAnimator != null)
            {
                myAnimator.SetBool("Active", true);
                myAnimator.Play("Active"); // sync up the animation
            }

            while (currentTime > 0)
            {
                yield return new WaitForSeconds(updateInterval);

                currentTime -= updateInterval;
                if (currentTime <= 0)
                {
                    currentTime = 0;
                }

                RefreshUI();
            }

            onCountdownComplete?.Invoke();

            if (myAnimator != null)
            {
                myAnimator.SetBool("Active", false);
            }

            if (otherObjectAnimator != null)
            {
                if (setOtherObjectAnimatorActive)
                {
                    otherObjectAnimator.gameObject.SetActive(true);
                }
                otherObjectAnimator.SetBool("Active", true);
            }

            yield return new WaitForSeconds(timeUpDuration);

            if (otherObjectAnimator != null)
            {
                otherObjectAnimator.SetBool("Active", false);
                if (setOtherObjectAnimatorActive)
                {
                    otherObjectAnimator.gameObject.SetActive(false);
                }
            }

            BeginTimer(0);
        }

        private void RefreshUI()
        {
            if (text)
            {
                text.SetText(currentTime.ToString(timerFormat));
            }
        }
    }
}

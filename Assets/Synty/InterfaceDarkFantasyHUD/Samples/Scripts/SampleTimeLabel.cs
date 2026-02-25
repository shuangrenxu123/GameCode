// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Synty.Interface.DarkFantasyHUD.Samples
{
    /// <summary>
    ///     A simple time label that displays the current time.
    /// </summary>
    public class SampleTimeLabel : MonoBehaviour
    {
        [Header("References")]
        public TMP_Text label;

        [Header("Parameters")]
        public bool is24Hour = true;
        public float timeToRefreshInSeconds = 1;

        private bool beat;

        private void Awake()
        {
            if (label == null)
            {
                label = GetComponent<TMP_Text>();
            }
        }

        private void OnEnable()
        {
            StartCoroutine(C_UpdateTime());
        }

        private void OnDisable()
        {
            StopCoroutine(C_UpdateTime());
        }

        public string GetCurrentTimeString()
        {
            if (!is24Hour)
            {
                return DateTime.Now.ToString("hh:mm tt");
            }

            if (beat)
            {
                return DateTime.Now.ToString("HH<color=#AAAAAA>:</color>mm");
            }

            return DateTime.Now.ToString("HH:mm");
        }

        [ContextMenu("Update Time")]
        public void UpdateTime()
        {
            label.SetText(GetCurrentTimeString());
        }

        private IEnumerator C_UpdateTime()
        {
            while (true)
            {
                UpdateTime();
                beat = !beat;
                yield return new WaitForSecondsRealtime(timeToRefreshInSeconds);
            }
        }
    }
}

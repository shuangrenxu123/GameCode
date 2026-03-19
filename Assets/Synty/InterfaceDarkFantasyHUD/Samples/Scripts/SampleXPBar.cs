// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Synty.Interface.DarkFantasyHUD.Samples
{
    /// <summary>
    ///     A simple XP bar that displays the current XP and level and continually levels-up indefinitely.
    /// </summary>
    public class SampleXPBar : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;
        public Slider xpSlider;
        public TMP_Text xpText;
        public TMP_Text levelText;

        [Header("Parameters")]
        public int xpPerLevelUp = 1000;

        private int currentLevel;
        private float currentXPNormalized;
        private float secondsPerLevelUp;

        private void Awake()
        {
            // Set random starting values.
            currentLevel = Random.Range(1, 69);
            currentXPNormalized = 0;
            secondsPerLevelUp = Random.Range(4, 20f);
        }

        private void Reset()
        {
            List<RectTransform> children = new List<RectTransform>();
            foreach (Transform child in transform)
            {
                if (child is RectTransform)
                {
                    children.Add(child as RectTransform);
                }
            }

            RectTransform xpObj = children.SingleOrDefault(c => c.name.ToLower().Contains("xp"));

            if (xpObj)
            {
                xpSlider = xpObj.GetComponentInChildren<Slider>();
                xpText = xpObj.transform.GetComponentInChildren<TMP_Text>();
            }

            RectTransform lvlObj = children.SingleOrDefault(c => c.name.ToLower().Contains("level"));

            if (lvlObj)
            {
                levelText = lvlObj.GetComponentInChildren<TMP_Text>();
            }
        }

        private void Update()
        {
            // Update the XP bar.
            if (xpSlider)
            {
                xpSlider.value = currentXPNormalized;
            }

            if (xpText)
            {
                xpText.text = $"{Mathf.RoundToInt(currentXPNormalized * xpPerLevelUp)}/{xpPerLevelUp}";
            }

            if (levelText)
            {
                levelText.text = $"{currentLevel}";
            }

            // If we have enough XP to level up, do so.
            if (currentXPNormalized >= 1f)
            {
                // LEVEL UP!
                currentLevel++;
                currentXPNormalized = 0;

                // Animate level up.
                if (animator)
                {
                    animator.SetTrigger("LevelUp");
                }
            }

            // Add XP based on time.
            currentXPNormalized += Time.deltaTime / secondsPerLevelUp;
        }
    }
}

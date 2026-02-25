// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System;
using UnityEngine;

namespace Synty.Interface.DarkFantasyHUD.Samples
{
    /// <summary>
    ///     A container class for Animator actions.
    /// </summary>
    [Serializable]
    public class SampleAnimatorActionData
    {
        public enum AnimatorActionType
        {
            Trigger,
            Bool,
            Float,
            Int
        }

        [Header("References")]
        public Animator animator;
        public AnimatorActionType type;

        [Header("Parameters")]
        public string parameterName;
        public bool boolToggle;
        public bool boolValue;
        public float floatValue;
        public int intValue;

        public void Execute()
        {
            if (!animator)
            {
                return;
            }

            animator.gameObject.SetActive(true);

            switch (type)
            {
                case AnimatorActionType.Trigger:
                    animator.SetTrigger(parameterName);
                    break;
                case AnimatorActionType.Bool:
                    if (boolToggle)
                    {
                        bool currentValue = animator.GetBool(parameterName);
                        animator.SetBool(parameterName, !currentValue);
                    }
                    else
                    {
                        animator.SetBool(parameterName, boolValue);
                    }

                    break;
                case AnimatorActionType.Float:
                    animator.SetFloat(parameterName, floatValue);
                    break;
                case AnimatorActionType.Int:
                    animator.SetInteger(parameterName, intValue);
                    break;
            }
        }
    }
}

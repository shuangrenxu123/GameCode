// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using UnityEngine;

namespace Synty.Tools.SyntyPropBoneTool
{
    /// <summary>
    ///     A configuration class used to define what bones are called, where they should be in the hierarchy and what offsets to use when updating them at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Synty/Animation/Synty Prop Bone Config", order = 1)]
    public class PropBoneConfig : ScriptableObject
    {
        [Tooltip("The rig used to create the animations. Rig must be in T Pose.")]
        public GameObject sourceRig;
        [Tooltip("The rig to play the animations on in Unity. Rig must be in T Pose.")]
        public GameObject targetRig;
        [Tooltip("Parameters that define where prop bones will generate and how to constrain them")]
        public PropBoneDefinition[] propBoneDefinitions;

        /// <summary>
        ///     Attempts to calculate the rotation and scale offset for all the bone definitions based on sourceRig and targetRig.
        ///     For the offsets to calculate correctly the sourceRig and targetRig must both be in T Pose.
        /// </summary>
        [ContextMenu("Calculate Offset Values")]
        public void CalculateOffsetValues()
        {
            if (sourceRig == null)
            {
                Debug.LogError($"Source rig is null in config {name}.", this);
            }
            if (targetRig == null)
            {
                Debug.LogError($"Target rig is null in config {name}.", this);
            }
            if (targetRig == null || sourceRig == null)
            {
                return;
            }

            for (int i = 0; i < propBoneDefinitions.Length; ++i)
            {
                // Check the bones in the definition exist in the source and target rigs
                Transform sourceParent = TransformUtil.SearchHierarchy(sourceRig.transform, propBoneDefinitions[i].parentBoneName);
                Transform targetParent = TransformUtil.SearchHierarchy(targetRig.transform, propBoneDefinitions[i].parentBoneName);

                if (sourceParent == null)
                {
                    Debug.LogError($"Cannot find bone in source rig called {propBoneDefinitions[i].parentBoneName}.");
                    continue;
                }
                if (targetParent == null)
                {
                    Debug.LogError($"Cannot find bone in target rig called {propBoneDefinitions[i].parentBoneName}.");
                    continue;
                }

                // Calculate the rotational offset between the source and target rigs
                propBoneDefinitions[i].rotationOffset = (Quaternion.Inverse(targetParent.rotation) * sourceParent.rotation).eulerAngles;

                // Check the scale calculation bones are defined and exist in the source and target rigs
                if (propBoneDefinitions[i].scaleCalculationBone1 == "" || propBoneDefinitions[i].scaleCalculationBone2 == "")
                {
                    continue;
                }
                Transform sourceScaleBone1 = TransformUtil.SearchHierarchy(sourceRig.transform, propBoneDefinitions[i].scaleCalculationBone1);
                Transform sourceScaleBone2 = TransformUtil.SearchHierarchy(sourceRig.transform, propBoneDefinitions[i].scaleCalculationBone2);
                Transform targetScaleBone1 = TransformUtil.SearchHierarchy(targetRig.transform, propBoneDefinitions[i].scaleCalculationBone1);
                Transform targetScaleBone2 = TransformUtil.SearchHierarchy(targetRig.transform, propBoneDefinitions[i].scaleCalculationBone2);

                if (sourceScaleBone1 == null)
                {
                    Debug.LogError($"Cannot find bone in source rig called {propBoneDefinitions[i].scaleCalculationBone1}.");
                    continue;
                }
                if (sourceScaleBone2 == null)
                {
                    Debug.LogError($"Cannot find bone in source rig called {propBoneDefinitions[i].scaleCalculationBone2}.");
                    continue;
                }
                if (targetScaleBone1 == null)
                {
                    Debug.LogError($"Cannot find bone in target rig called {propBoneDefinitions[i].scaleCalculationBone1}.");
                    continue;
                }
                if (targetScaleBone2 == null)
                {
                    Debug.LogError($"Cannot find bone in target rig called {propBoneDefinitions[i].scaleCalculationBone2}.");
                    continue;
                }

                // Calculate the scale offset between the source and target rigs
                propBoneDefinitions[i].scale = 1;
                float sourceLength = Vector3.Distance(sourceScaleBone1.position, sourceScaleBone2.position);
                float targetLength = Vector3.Distance(targetScaleBone1.position, targetScaleBone2.position);

                if (sourceLength < float.Epsilon)
                {
                    Debug.LogError($"Distance betweem source rig's scale bones is zero: scale bone 1: {propBoneDefinitions[i].scaleCalculationBone1}, scale bone 2 {propBoneDefinitions[i].scaleCalculationBone2}.");
                    continue;
                }
                if (targetLength < float.Epsilon)
                {
                    Debug.LogError($"Distance betweem target rig's scale bones is zero: scale bone 1: {propBoneDefinitions[i].scaleCalculationBone1}, scale bone 2 {propBoneDefinitions[i].scaleCalculationBone2}.");
                    continue;
                }

                propBoneDefinitions[i].scale = targetLength / sourceLength;
            }
        }
    }
}
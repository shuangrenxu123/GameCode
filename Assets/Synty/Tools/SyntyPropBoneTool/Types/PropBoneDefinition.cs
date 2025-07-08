// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using System;
using UnityEngine;

namespace Synty.Tools.SyntyPropBoneTool
{
    /// <summary>
    ///     Defines the parameters used by the PropBoneBinder to create the required bones and calculate the parameters used to update those bones at runtime.
    /// </summary>
    [Serializable]
    public struct PropBoneDefinition
    {
        [Tooltip("The name of the bone in your character's rig to attach the prop bone.")]
        public string parentBoneName;
        [Tooltip("The name of the prop bone to instantiate.")]
        public string boneName;
        [Tooltip("The name of the additional transform created to attach props under.")]
        public string socketName;

        [Tooltip("Rotation offset used to compensate for differences in orientation of the parent bone between the source rig and the target rig.")]
        public Vector3 rotationOffset;
        [Tooltip("Scalar used to compensate for differences in size between the source rig and the target rig. 1 = target rig is the same scale as the reference rig, 2 = target rig is twice the size as the reference rig.")]
        public float scale;

        [Tooltip("Bone to use to calculate scalar value automatically.")]
        public string scaleCalculationBone1;
        [Tooltip("Bone to use to calculate scalar value automatically.")]
        public string scaleCalculationBone2;
    }
}
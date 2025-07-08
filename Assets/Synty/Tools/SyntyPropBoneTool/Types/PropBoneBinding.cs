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
    ///     Defines the parameters used to update the prop bone at runtime.
    /// </summary>
    [Serializable]
    public class PropBoneBinding
    {
        // Public so they can be see in the inspector for debug purposes
        public Transform bone;
        public Transform socket;

        public Vector3 rotationOffset;
        public float scale = 1;

        // Returns true if the prop bone binding is able to be updated
        public bool IsValid { get { return bone != null && socket != null; } }

        /// <summary>
        ///     Compares the parameters of this prop bone binding with the given prop bone binding.
        /// </summary>
        /// <param name="other">The PropBoneDefinition to compare with.</param>
        /// <returns>A <c>bool</c>. True when the bindings are equvilent.</returns>
        public bool IsMatch(PropBoneDefinition other)
        {
            return bone.name == other.boneName
                && socket.name == other.socketName
                && (bone.parent != null ? bone.parent.name == other.parentBoneName : string.IsNullOrEmpty(other.parentBoneName))
                && rotationOffset == other.rotationOffset && scale == other.scale;
        }
    }
}
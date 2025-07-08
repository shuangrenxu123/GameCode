
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
    ///     Hard coded default values for PropBoneDefinitions.
    /// </summary>
    public static class PropBoneDefinitionPresets
    {
        // Preset configs for Synty rigs
        public static PropBoneDefinition[] PolygonBoneDefinition
        {
            get
            {
                return new PropBoneDefinition[]
                {
                    new PropBoneDefinition() {
                        parentBoneName = "Hand_R",
                        boneName = "Prop_R",
                        socketName = "Prop_R_Socket",
                        rotationOffset = new Vector3(0,0,0),
                        scale = 1f,
                        scaleCalculationBone1 = "Hand_R",
                        scaleCalculationBone2 = "Elbow_R"
                    }
                };
            }
        }
    }
}
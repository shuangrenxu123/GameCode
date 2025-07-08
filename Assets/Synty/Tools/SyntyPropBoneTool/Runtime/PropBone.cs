// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using UnityEngine;

namespace Synty.Tools.SyntyPropBoneTool
{
    // This is a patch that is only required if the rig does not match POLYGON Animation Packs' rigs
    public class PropBone : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private bool _wasSpawnedBySyntyTool = true;
        public bool WasSpawnedBySyntyTool { get { return _wasSpawnedBySyntyTool; } set { _wasSpawnedBySyntyTool = value; } }
    }
}
// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using System;

namespace Synty.Tools.SyntyPropBoneTool
{
    [Serializable]
    public enum UpdateType
    {
        Update,
        FixedUpdate,
        LateUpdate,
        Manual
    }
}
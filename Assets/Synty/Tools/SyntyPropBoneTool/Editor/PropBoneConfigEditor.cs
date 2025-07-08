// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Synty.Tools.SyntyPropBoneTool
{
    /// <summary>
    ///     Controls UI interactions for the selected PropBoneConfig asset.
    /// </summary>
    [CustomEditor(typeof(PropBoneConfig))]
    [CanEditMultipleObjects]
    public class PropBoneConfigEditor : Editor
    {
        private List<PropBoneConfig> _configs = new List<PropBoneConfig>();

        /// <summary>
        ///     Sets up the _configs based on the current targets.
        /// </summary>
        private void OnEnable()
        {
            _configs.Clear();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is PropBoneConfig)
                {
                    _configs.Add(targets[i] as PropBoneConfig);
                }
            }
        }

        /// <summary>
        ///     Draws the Unity editor gui for the selected PropBoneConfig assets.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(20);
            if (GUILayout.Button("Calculate Bone Offset Values", GUILayout.Height(30)))
            {
                for (int i = 0; i < _configs.Count; ++i)
                {
                    _configs[i].CalculateOffsetValues();
                }
            }
        }
    }
}
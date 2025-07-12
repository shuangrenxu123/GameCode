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
    ///     Controls UI interactions for the selected PropBoneBinder components.
    /// </summary>
    [CustomEditor(typeof(PropBoneBinder))]
    [CanEditMultipleObjects]
    public class PropBoneBinderEditor : Editor
    {
        private List<PropBoneBinder> _binders = new List<PropBoneBinder>();
        private static bool _moreOptionsFoldout = false;

        /// <summary>
        ///     Sets up the binders based on the current targets.
        /// </summary>
        private void OnEnable()
        {
            _binders.Clear();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is PropBoneBinder)
                {
                    _binders.Add(targets[i] as PropBoneBinder);
                }
            }
        }

        /// <summary>
        ///     Draws the Unity editor gui for the selected PropBoneBinder components.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUIStyle statusStyle = new GUIStyle(EditorStyles.helpBox);
            string status = "";
            bool editorDisabled = false;

            // help text
            if (_binders.Count == 1)
            {
                editorDisabled = PropBoneBinderEditorUtil.IsPrefabAsset(_binders[0]);

                if (_binders[0].animator != null)
                {
                    if (_binders[0].propBoneConfig != null)
                    {
                        if (_binders[0].IsPropBoneHierarchyConfigured())
                        {
                            if (_binders[0].AreBindingsConfigured())
                            {
                                statusStyle.normal.textColor = Color.green;
                                status = "This character has been set up!";
                            }
                            else
                            {
                                statusStyle.normal.textColor = Color.yellow;
                                status = "Bindings are not configured. You may need to bind the prop bones.";
                            }
                        }
                        else
                        {
                            statusStyle.normal.textColor = Color.yellow;
                            status = "Prop bones are not configured. You may need to create prop bones.";
                        }
                    }
                    else
                    {
                        statusStyle.normal.textColor = Color.red;
                        status = "Prop bone config is null. Setup a prop bone config.";
                    }
                }
                else
                {
                    statusStyle.normal.textColor = Color.red;
                    status = "This character is not set up. Try 'One Click Setup'.";
                }
            }
            else
            {
                int configuredCount = 0;
                for (int i = 0; i < _binders.Count; ++i)
                {
                    editorDisabled = editorDisabled || PropBoneBinderEditorUtil.IsPrefabAsset(_binders[i]);

                    if (_binders[i].IsConfigured)
                    {
                        configuredCount++;
                    }
                }
                if (configuredCount < _binders.Count)
                {
                    statusStyle.normal.textColor = Color.red;
                    status = $"{configuredCount} of the {_binders.Count} selected characters are set up.";
                }
                else
                {
                    statusStyle.normal.textColor = Color.green;
                    status = $"All {_binders.Count} selected characters are set up.";
                }
            }
            EditorGUILayout.TextField("Setup Status", status, statusStyle);
            GUILayout.Space(20);

            if (editorDisabled)
            {
                EditorGUILayout.LabelField("Open prefab in edit mode or create an instance in the scene to set up prop bones.");
            }
            EditorGUI.BeginDisabledGroup(editorDisabled);
            {
                // quick setup options
                if (GUILayout.Button("One Click Setup", GUILayout.Height(50)))
                {
                    PropBoneBinderEditorUtil.AutomaticSetup(_binders);
                }
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(20);
            DrawDefaultInspector();

            // More Options
            GUILayout.Space(20);
            _moreOptionsFoldout = EditorGUILayout.Foldout(_moreOptionsFoldout, "More Options");
            if (_moreOptionsFoldout)
            {
                EditorGUI.indentLevel++;

                if (GUILayout.Button("Create New Bone Config Asset", GUILayout.Height(40)))
                {
                    PropBoneBinderEditorUtil.CreateNewBoneConfigs(_binders);
                }

                GUILayout.Space(20);
                if (editorDisabled)
                {
                    EditorGUILayout.LabelField("Open prefab in edit mode or create an instance in the scene to edit prop bones.");
                }
                EditorGUI.BeginDisabledGroup(editorDisabled);
                {
                    if (GUILayout.Button("Setup Animator Reference", GUILayout.Height(30)))
                    {
                        PropBoneBinderEditorUtil.SetupAnimatorReferences(_binders);
                    }

                    if (GUILayout.Button("Setup Prop Bone Config", GUILayout.Height(30)))
                    {
                        PropBoneBinderEditorUtil.SetupPropBoneConfigs(_binders);
                    }

                    if (GUILayout.Button("Create Prop Bones", GUILayout.Height(30)))
                    {
                        PropBoneBinderEditorUtil.CreatePropBones(_binders);
                    }

                    if (GUILayout.Button("Bind Prop Bones", GUILayout.Height(30)))
                    {
                        PropBoneBinderEditorUtil.BindPropBones(_binders);
                    }

                    GUILayout.Space(20);
                    if (GUILayout.Button("Reset", GUILayout.Height(30)))
                    {
                        PropBoneBinderEditorUtil.AutomaticReset(_binders);
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        ///     Attempts to perform all the necessary steps required to configure the selected characters with prop bones.
        /// </summary>
        [MenuItem("Synty/Tools/Animation/Setup Prop Bones")]
        private static void SetupSyntyPropBones()
        {
            List<PropBoneBinder> binders = new List<PropBoneBinder>();
            for (int i = 0; i < Selection.objects.Length; ++i)
            {
                if (Selection.objects[i] is GameObject)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(Selection.objects[i]))
                    {
                        Debug.LogWarning($"Cannot edit prefab asset {Selection.objects[i].name}. Open the asset in prefab edit mode or create a scene instance and try again.", Selection.objects[i]);
                        continue;
                    }

                    GameObject gameObject = Selection.objects[i] as GameObject;
                    Animator animator = gameObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        PropBoneBinder binder = gameObject.GetComponent<PropBoneBinder>();
                        if (binder == null)
                        {
                            binder = gameObject.AddComponent<PropBoneBinder>();
                        }

                        if (binder != null)
                        {
                            Debug.Log($"Setting up prop bones for game object.", gameObject);
                            binders.Add(binder);
                        }
                    }
                }
            }

            if (binders.Count > 0)
            {
                PropBoneBinderEditorUtil.AutomaticSetup(binders);
            }
            else
            {
                Debug.LogError("Select some characters and try again.");
            }
        }
    }
}
// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

namespace Synty.Tools.SyntyPropBoneTool
{
    /// <summary>
    ///     The PropBoneBinder is responsible for creating, managing and updating the prop bones at runtime.
    ///     Only one of these components is needed per character. It will manage all prop bones defined in the propBoneConfig.
    /// </summary>
    public class PropBoneBinder : MonoBehaviour
    {
        [Tooltip("Reference to the animator for this character")]
        public Animator animator;

        [Tooltip("Configures how the bones are set up on the rig.")]
        public PropBoneConfig propBoneConfig;

        [Tooltip("Determins when this script will update the transforms. For best results run this script later than the animator.")]
        public UpdateType updateType = UpdateType.LateUpdate;

        [Tooltip("Rebinds all the bones on awake. Useful if your rigs change often, saves needing to rebind them at edit time.")]
        public bool rebindOnAwake = false;

        [Space]
        [Tooltip("Bindings and offset values applied at runtime.")]
        [SerializeField]
        private List<PropBoneBinding> _propBoneBindings = new List<PropBoneBinding>();

        /// <summary>
        ///     Returns true if the PropBoneBinder is configured correctly.
        /// </summary>
        public bool IsConfigured => AreReferencesConfigured() && AreBindingsConfigured();

        /// <summary>
        ///     Returns true if the reference variables are configured correctly.
        /// </summary>
        /// <returns>A <c>bool</c> that is true when the reference variables are correctly configured.</returns>
        public bool AreReferencesConfigured()
        {
            return animator != null && propBoneConfig != null;
        }

        /// <summary>
        ///     Returns true if the hierarchy is configured correctly according to the propBoneConfig.
        /// </summary>
        /// <returns>A <c>bool</c> that is true when the hierarchy is correctly configured.</returns>
        public bool IsPropBoneHierarchyConfigured()
        {
            for (int i = 0; i < propBoneConfig.propBoneDefinitions.Length; ++i)
            {
                Transform propBone = TransformUtil.SearchHierarchy(transform, propBoneConfig.propBoneDefinitions[i].boneName);
                Transform propBoneSocket = TransformUtil.SearchHierarchy(transform, propBoneConfig.propBoneDefinitions[i].socketName);
                if (propBone == null || propBoneSocket == null || propBone.parent == null || propBone.parent.name != propBoneConfig.propBoneDefinitions[i].parentBoneName)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns true if all the prop bone bindings are configured correctly according to the propBoneConfig.
        /// </summary>
        /// <returns>A <c>bool</c> that is true when all the prop bone bindings are correctly configured.</returns>
        public bool AreBindingsConfigured()
        {
            if (_propBoneBindings.Count != propBoneConfig.propBoneDefinitions.Length)
            {
                return false;
            }

            for (int i = 0; i < _propBoneBindings.Count; ++i)
            {
                if (!_propBoneBindings[i].IsMatch(propBoneConfig.propBoneDefinitions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Rebinds the PropBoneBindings on awake if rebindOnAwake is set to true.
        /// </summary>
        private void Awake()
        {
            if (rebindOnAwake)
            {
                BindPropBones();
            }
        }

        /// <summary>
        ///     Attempts to create a PropBoneBinding in accordance to the giving PropBoneDefinition.
        /// </summary>
        /// <param name="boneDefinition">The definition of the prop bone to be created.</param>
        /// <returns>A <c>PropBoneBinding</c> that is the new binding or null if the binding failed to be created.</returns>
        private PropBoneBinding CreateBoneBinding(PropBoneDefinition boneDefinition)
        {
            Transform parent = TransformUtil.SearchHierarchy(transform, boneDefinition.parentBoneName);
            if (parent == null)
            {
                Debug.LogError($"Cannot find parent bone {boneDefinition.parentBoneName}.", transform);
                return null;
            }

            Transform bone = TransformUtil.SearchHierarchy(transform, boneDefinition.boneName);
            if (bone == null)
            {
                Debug.LogError($"Cannot find bone {boneDefinition.boneName}.", transform);
            }
            else if (bone.parent.name != boneDefinition.parentBoneName)
            {
                Debug.LogError($"bone.parent {bone.parent.name} does not match {boneDefinition.parentBoneName} in hierarchy.", bone);
                return null;
            }

            Transform socket = TransformUtil.SearchHierarchy(transform, boneDefinition.socketName);
            if (socket == null)
            {
                Debug.LogError($"Cannot find socket {boneDefinition.socketName}.", transform);
                return null;
            }
            else if (socket.parent != bone)
            {
                Debug.LogError($"socket {socket.name} is not parented to bone {bone.name}.", socket);
                return null;
            }

            PropBoneBinding binding = new PropBoneBinding()
            {
                bone = bone,
                socket = socket,
                rotationOffset = boneDefinition.rotationOffset,
                scale = boneDefinition.scale
            };

            return binding;
        }

        /// <summary>
        ///     Updates the prop bones if updateType is set to 'Update'.
        /// </summary>
        private void Update()
        {
            if (updateType == UpdateType.Update)
            {
                UpdateBones();
            }
        }

        /// <summary>
        ///     Updates the prop bones if updateType is set to 'FixedUpdate'.
        /// </summary>
        private void FixedUpdate()
        {
            if (updateType == UpdateType.FixedUpdate)
            {
                UpdateBones();
            }
        }

        /// <summary>
        ///     Updates the prop bones if updateType is set to 'LateUpdate'.
        /// </summary>
        private void LateUpdate()
        {
            if (updateType == UpdateType.LateUpdate)
            {
                UpdateBones();
            }
        }

        /// <summary>
        ///     Updates the prop bones according to the prop bone bindings configuration.
        ///     If updateType is set to 'Manual' call this yourself when it is time to update the bones.
        /// </summary>
        public void UpdateBones()
        {
            for (int index = 0; index < _propBoneBindings.Count; ++index)
            {
                UpdateBone(_propBoneBindings[index]);
            }
        }


        /// <summary>
        ///     Updates the prop bone's position and rotation according to the prop bone binding configuration.
        /// </summary>
        /// <param name="boneInstance">The prop bone binding to update.</param>
        public void UpdateBone(PropBoneBinding boneInstance)
        {
            if (boneInstance.IsValid)
            {
                Quaternion offsetRotation = Quaternion.Euler(boneInstance.rotationOffset);
                Matrix4x4 localRotation = Matrix4x4.Rotate(offsetRotation);
                Matrix4x4 localScale = Matrix4x4.Scale(Vector3.one * boneInstance.scale);

                Matrix4x4 localTransform = localScale * localRotation;

                boneInstance.socket.SetPositionAndRotation(
                    boneInstance.bone.parent.localToWorldMatrix.MultiplyPoint(localTransform.MultiplyPoint(boneInstance.bone.localPosition)),
                    boneInstance.bone.parent.rotation * offsetRotation * boneInstance.bone.localRotation);
            }
        }

        /// <summary>
        ///     Attempts to find and set the animator reference.
        /// </summary>
        public void SetupAnimatorReference()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            if (animator == null)
            {
                Debug.LogError($"Animator reference is null. New bones may not bind correctly.", transform);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
        }

        /// <summary>
        ///     Completely resets the prop bones and bindings including Destroying all bones instantiated by the PropBoneBinder.
        ///     Any objects parented to those bones will not be destroyed and will be reparented up the hierarchy.
        /// </summary>
        public void Reset()
        {
            animator = null;
            propBoneConfig = null;
            ClearPropBoneBindings();
            DestroyPropBones();
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
        }

        /// <summary>
        ///     Attempts to create all the PropBoneBindings in accordance to the propBondConfig
        /// </summary>
        public void BindPropBones()
        {
            if (animator == null)
            {
                Debug.LogError($"animator reference is null.", transform);
            }

            _propBoneBindings.Clear();

            if (propBoneConfig == null)
            {
                Debug.LogError($"Prop bone config is null.", transform);
                return;
            }

            for (int i = 0; i < propBoneConfig.propBoneDefinitions.Length; ++i)
            {
                PropBoneBinding binding = CreateBoneBinding(propBoneConfig.propBoneDefinitions[i]);
                if (binding == null)
                {
                    Debug.LogError($"Could not create binding for prop bone definition {propBoneConfig.propBoneDefinitions[i].ToString()}.", transform);
                    continue;
                }

                _propBoneBindings.Add(binding);
                if (binding.bone != null && binding.socket != null)
                {
                    Debug.Log($"Successfully bound {binding.socket.name} to {binding.bone.name}", binding.socket);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
        }

        /// <summary>
        ///     Clears all the current bindings
        /// </summary>
        public void ClearPropBoneBindings()
        {
            _propBoneBindings.Clear();

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
        }

        /// <summary>
        ///     Instantiates new Game Objects where nessessary to create the prop bones.
        /// </summary>
        public void CreatePropBones()
        {
            if (propBoneConfig == null)
            {
                Debug.LogError($"Prop bone config is null.", transform);
                return;
            }

            for (int i = 0; i < propBoneConfig.propBoneDefinitions.Length; ++i)
            {
                CreatePropBones(gameObject, propBoneConfig.propBoneDefinitions[i]);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
        }

        /// <summary>
        ///     Instantiates new Game Objects where nessessary to create the prop bones.
        /// </summary>
        /// <param name="editScope">The root game object to create the prop bone under.</param>
        /// <param name="boneDefinition">The definition of the prop bone to be created.</param>
        /// <param name="boneInstance">The prop bone binding to update.</param>
        private void CreatePropBones(GameObject editScope, PropBoneDefinition boneDefinition)
        {
            Transform parent = TransformUtil.SearchHierarchy(editScope.transform, boneDefinition.parentBoneName);
            if (parent == null)
            {
                Debug.LogError($"Cannot find parent prop bone {boneDefinition.parentBoneName} in hierarchy.", transform);
                return;
            }

            Transform bone = TransformUtil.SearchHierarchy(editScope.transform, boneDefinition.boneName);
            if (bone == null)
            {
                bone = CreatePropBone(boneDefinition.boneName, parent);
                if (bone == null)
                {
                    return;
                }
            }
            else if (bone.parent.name != boneDefinition.parentBoneName)
            {
                Debug.LogError($"bone.parent {bone.parent.name} does not match {boneDefinition.parentBoneName} in hierarchy.", bone);
                return;
            }

            Transform socket = TransformUtil.SearchHierarchy(editScope.transform, boneDefinition.socketName);
            if (socket == null)
            {
                socket = CreatePropBone(boneDefinition.socketName, bone);
            }
            else if (socket.parent != bone)
            {
                Debug.LogError($"socket {socket.name} is not parented to bone {bone.name}.", socket);
                return;
            }
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(socket);
#endif
        }

        /// <summary>
        ///     Instantiates a new Game Object called 'name' and parents it to 'parent'.
        /// </summary>
        /// <param name="name">The name of the new bone to be created.</param>
        /// <param name="parent">The transform to parent the new bone to.</param>
        private Transform CreatePropBone(string name, Transform parent)
        {
            // does not check if there is already a bone called this. we should check before calling create
            Transform boneInstance = new GameObject(name).transform;
            boneInstance.SetParent(parent);
            if (boneInstance.parent != parent)
            {
                Debug.LogError($"Something went wrong when creating prop bone {boneInstance.name}. You may need to enter prefab edit mode to set up this character.", gameObject);
                DestroyImmediate(boneInstance.gameObject);
                return null;
            }
            boneInstance.localPosition = Vector3.zero;
            boneInstance.localRotation = Quaternion.identity;
            boneInstance.localScale = Vector3.one;
            // mark that these have been created by the tool
            boneInstance.gameObject.AddComponent<PropBone>().WasSpawnedBySyntyTool = true;

            Debug.Log($"Successfully created prop bone {boneInstance.name}.", boneInstance);

            return boneInstance;
        }

        /// <summary>
        ///     Destroys all the prop bones that have been created by the PropBoneBinder and clears all bindings.
        /// </summary>
        public void DestroyPropBones()
        {
            DestroyPropBones(gameObject);
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
        }

        /// <summary>
        ///     Destroys all the prop bones that have been created by the PropBoneBinder and clears all bindings
        /// </summary>
        /// <param name="editScope">The root game objects to destroy prop bones from.</param>
        private void DestroyPropBones(GameObject editScope)
        {
            _propBoneBindings.Clear();
            PropBone[] bones = editScope.GetComponentsInChildren<PropBone>();
            for (int i = 0; i < bones.Length; ++i)
            {
                if (bones[i].WasSpawnedBySyntyTool)
                {
                    // Let's not destroy peoples swords and stuff.
                    for (int c = 0; c < bones[i].transform.childCount; ++c)
                    {
                        Transform child = bones[i].transform.GetChild(c);
                        child.SetParent(bones[i].transform.parent);
                        bool hasPropBone = child.GetComponent<PropBone>() != null;
                        if (!hasPropBone)
                        {
                            Debug.Log($"Successfully reparented object {child.name}.", child);
                        }
                    }

                    string boneName = bones[i].gameObject.name;
                    Transform parent = bones[i].transform.parent;

                    if (Application.isEditor && !Application.isPlaying)
                    {
                        DestroyImmediate(bones[i].gameObject);
                    }
                    else
                    {
                        Destroy(bones[i].gameObject);
                    }

                    if (parent != null)
                    {
                        Debug.Log($"Successfully destroyed bone {boneName} under {parent.name}.", parent);
                    }
                    else
                    {
                        Debug.Log($"Successfully destroyed bone {boneName}.");
                    }
                }
                else
                {
                    GameObject gameObject = bones[i].gameObject;
                    // Remove the component but don't destroy the object.
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        DestroyImmediate(bones[i]);
                    }
                    else
                    {
                        Destroy(bones[i]);
                    }

                    if (gameObject != null)
                    {
                        Debug.Log($"Successfully removed SyntyPropBone component from {gameObject.name}.", gameObject);
                    }
                }
            }
        }
    }
}
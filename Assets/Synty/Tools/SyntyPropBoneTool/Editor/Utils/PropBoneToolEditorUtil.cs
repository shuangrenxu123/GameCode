// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Synty.Tools.SyntyPropBoneTool
{
    /// <summary>
    ///     Utility class to help configure many characters with prop bone binders at once.
    /// </summary>
    public static class PropBoneBinderEditorUtil
    {
        // Path where the prop bone binder tool stores the default config file.
        private const string CONFIG_ASSET_NAME_DEFAULT = "AnimationSwordCombat_PropBoneBindingConfig_Default.asset";
        private const string CONFIG_ASSET_PATH_DEFAULT = FOLDER_PATH_DEFAULT + CONFIG_ASSET_NAME_DEFAULT;

        // Folder path where the prop bone binder tool stores config files by default.
        private const string FOLDER_PATH_DEFAULT = "Assets/Synty/Tools/SyntyPropBoneTool/Configs/";

        /// <summary>
        ///     Generates a file name based on the targetRigName and the defined config folder path.
        /// </summary>
        /// <returns>A <c>string</c> that can be used as a file path for a config asset. This path is not gauranteed to be unique.</returns>
        private static string GenerateNewConfigFileName(string targetRigName)
        {
            PropBoneConfigAsset defaultAsset = GetDefaultConfigAsset();

            string targetFileName = targetRigName + "_PropBoneBindingConfig.asset";
            string targetPath = defaultAsset.path.Replace(CONFIG_ASSET_NAME_DEFAULT, targetFileName);

            return targetPath;
        }

        /// <summary>
        ///     Class used to help manage config assets in the AssetDatabase.
        /// </summary>
        private class PropBoneConfigAsset
        {
            public bool savedInAssetDatabase; // is true when loaded from or saved to the AssetDatabase
            public PropBoneConfig config;
            public string path;
        }

        /// <summary>
        ///     Returns true if the given PropBneBinder is part of a prefab asset.
        /// </summary>
        /// <param name="binder">The PropBoneBinder to test if it is part of a prefab asset.</param>
        /// <returns>A <c>bool</c>. True when the binder is part of a prefab asset.</returns>
        public static bool IsPrefabAsset(PropBoneBinder binder)
        {
            return PrefabUtility.IsPartOfPrefabAsset(binder);
        }

        /// <summary>
        ///     Attemps to automatically configure all given PropBoneBinders.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to setup.</param>
        public static void AutomaticSetup(List<PropBoneBinder> binders)
        {
            SetupAnimatorReferences(binders);
            SetupPropBoneConfigs(binders);
            CreatePropBones(binders);
            BindPropBones(binders);
        }

        /// <summary>
        ///     Attemps to reset all given PropBoneBinders.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to reset.</param>
        public static void AutomaticReset(List<PropBoneBinder> binders)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                binders[i].Reset();
            }
        }

        /// <summary>
        ///     Attempts to find the first PropBoneConfigAsset with the given fileName.
        /// </summary>
        /// <param name="fileName">The file name of the config to find.</param>
        /// <returns>The first found <c>PropBoneConfigAsset</c> with file name matching the given fileName or null if none is found.</returns>
        private static PropBoneConfigAsset FindFirstConfig(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets("t:PropBoneConfig");
            for (int i = 0; i < guids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.Contains(fileName))
                {
                    return LoadPropBoneConfig(path);
                }
            }

            return null;
        }

        /// <summary>
        ///     Attempts to load the default PropBoneConfigAsset or if one is not found creates a new default PropBoneConfigAsset.
        /// </summary>
        /// <returns>A <c>PropBoneConfigAsset</c> that contains the default settings to use for PropBoneBingings.</returns>
        private static PropBoneConfigAsset GetDefaultConfigAsset()
        {
            PropBoneConfigAsset defaultConfigAsset = FindFirstConfig(CONFIG_ASSET_NAME_DEFAULT);

            if (defaultConfigAsset == null)
            {
                PropBoneConfig defaultConfig = CreatePropBoneConfig(PropBoneDefinitionPresets.PolygonBoneDefinition);
                defaultConfigAsset = CreatePropBoneConfigAsset(defaultConfig, CONFIG_ASSET_PATH_DEFAULT);
                SavePropBoneAssetToProject(defaultConfigAsset);
            }

            return defaultConfigAsset;
        }

        /// <summary>
        ///     Attempts to load the default PropBoneConfig or if one is not found creates a new default PropBoneConfig.
        /// </summary>
        /// <returns>A <c>PropBoneConfig</c> that contains the default settings to use for PropBoneBingings.</returns>
        private static PropBoneConfig GetDefaultConfig()
        {
            return GetDefaultConfigAsset().config;
        }

        /// <summary>
        ///     Attempts to find and load a config that matched the given sourceRig and targetRig.
        /// </summary>
        /// <param name="sourceRig">The source rig to match when finding the PropBoneConfig.</param>
        /// <param name="targetRig">The target rig to match when finding the PropBoneConfig.</param>
        /// <returns>A <c>PropBoneConfig</c> that matches the sourceRig and targetRig or returns the default PropBoneConfig if a match is not found.</returns>
        public static PropBoneConfig FindFirstMatchingConfig(GameObject sourceRig, GameObject targetRig)
        {
            string[] guids = AssetDatabase.FindAssets("t:PropBoneConfig");
            for (int i = 0; i < guids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                PropBoneConfig config = LoadPropBoneConfig(path).config;
                if (config != null)
                {
                    if (config.targetRig == targetRig && config.sourceRig == sourceRig)
                    {
                        return config;
                    }
                }
            }

            return GetDefaultConfig();
        }

        /// <summary>
        ///     Attempts to find matching PropBoneConfigs or creates a new ones and assigns them to all the given PropBoneBinders.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to setup.</param>
        public static void SetupPropBoneConfigs(List<PropBoneBinder> binders)
        {
            PropBoneConfig defaultConfig = GetDefaultConfig();
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                if (binders[i].propBoneConfig == null)
                {
                    GameObject targetRig = null;
                    if (binders[i].animator != null)
                    {
                        targetRig = binders[i].animator.gameObject;
                    }

                    if (PrefabUtility.IsPartOfPrefabInstance(targetRig))
                    {
                        string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(targetRig);
                        if (path != null)
                        {
                            targetRig = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        }
                    }
                    else if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        string path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
                        if (path != null)
                        {
                            targetRig = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        }
                    }

                    if (targetRig == null)
                    {
                        Debug.LogError($"Cannot locate target model asset for binder: {binders[i].gameObject.name}. You will need to set up the prop bone config for this character manually and then run setup again.", binders[i].gameObject);
                        continue;
                    }

                    PropBoneConfig targetConfig = FindFirstMatchingConfig(defaultConfig.sourceRig, targetRig);

                    if (targetConfig.targetRig != targetRig)
                    {
                        PropBoneConfig newConfig = ClonePropBoneConfig(defaultConfig);
                        newConfig.targetRig = targetRig;
                        newConfig.CalculateOffsetValues();
                        Debug.Log($"Set up {targetRig.name} as target rig for {binders[i].gameObject.name}", targetRig);
                        PropBoneConfigAsset configAsset = CreatePropBoneConfigAsset(newConfig, GenerateNewConfigFileName(targetRig.name));
                        SavePropBoneAssetToProject(configAsset);
                        targetConfig = configAsset.config;
                    }

                    binders[i].propBoneConfig = targetConfig;
                }
            }
        }

        /// <summary>
        ///     Creates new PropBoneConfig files based on the ones assigned to the given binders or bases new configs on the default PropBoneConfig if none are assigned.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to create new bone configs for.</param>
        public static void CreateNewBoneConfigs(List<PropBoneBinder> binders)
        {
            PropBoneConfig defaultConfig = GetDefaultConfig();
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                PropBoneConfig newConfigBase = binders[i].propBoneConfig != null ? binders[i].propBoneConfig : defaultConfig;

                PropBoneConfig newConfig = ClonePropBoneConfig(newConfigBase);
                PropBoneConfigAsset configAsset = CreatePropBoneConfigAsset(newConfig, GenerateNewConfigFileName(binders[i].name));
                SavePropBoneAssetToProject(configAsset);
                binders[i].propBoneConfig = configAsset.config;
            }
        }

        /// <summary>
        ///     Attempts to assign the animator referenec on all the given binders.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to setup the animator references of.</param>
        public static void SetupAnimatorReferences(List<PropBoneBinder> binders)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                PropBoneBinder binder = binders[i];
                if (binder != null)
                {
                    binder.SetupAnimatorReference();
                }
            }
        }

        /// <summary>
        ///     Attempts to create all the prop bones for all the given binders.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to create prop bones for.</param>
        public static void CreatePropBones(List<PropBoneBinder> binders)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                PropBoneBinder binder = binders[i];
                if (binder != null)
                {
                    binder.CreatePropBones();
                }
            }
        }

        /// <summary>
        ///     Attempts to clear all the prop bone bindings for all the given binders.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to create prop bones bindings for.</param>
        public static void ClearPropBoneBindings(List<PropBoneBinder> binders)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                PropBoneBinder binder = binders[i];
                if (binder != null)
                {
                    binder.ClearPropBoneBindings();
                }
            }
        }

        /// <summary>
        ///     Attempts to destroy all the prop bones on all the given bindings.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to destroy prop bones.</param>
        public static void DestroyPropBones(List<PropBoneBinder> binders)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                PropBoneBinder binder = binders[i];
                if (binder != null)
                {
                    binder.DestroyPropBones();
                }
            }
        }

        /// <summary>
        ///     Attempts to bind all the prop bones on all the given bindings.
        /// </summary>
        /// <param name="binders">All the PropBoneBinder components to bind prop bones.</param>
        public static void BindPropBones(List<PropBoneBinder> binders)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                if (IsPrefabAsset(binders[i]))
                {
                    Debug.LogWarning($"Cannot edit prefab asset {binders[i].gameObject.name}. Open the asset in prefab edit mode or create a scene instance and try again.", binders[i].gameObject);
                    continue;
                }

                PropBoneBinder binder = binders[i];
                if (binder != null)
                {
                    binder.BindPropBones();
                    binder.UpdateBones();
                }
            }
        }

        /// <summary>
        ///     Loads a PropBoneConfig file at the given path.
        /// </summary>
        /// <param name="path">The path to load the PropBoneConfig.</param>
        /// <returns>A <c>PropBoneConfigAsset</c> found at the given path or null if no asset of type PropBoneConfigAsset exists at that path.</returns>
        private static PropBoneConfigAsset LoadPropBoneConfig(string path)
        {
            PropBoneConfig config = AssetDatabase.LoadAssetAtPath<PropBoneConfig>(path);
            if (config != null)
            {
                PropBoneConfigAsset configAsset = new PropBoneConfigAsset();
                configAsset.config = config;
                configAsset.savedInAssetDatabase = true;
                configAsset.path = path;
                return configAsset;
            }

            return null;
        }

        /// <summary>
        ///     Creates a new PropBoneConfig based on the given source config.
        /// </summary>
        /// <param name="source">The PropBoneConfig to clone.</param>
        /// <returns>A new <c>PropBoneConfig</c> clones from the source PropBoneConfig.</returns>
        private static PropBoneConfig ClonePropBoneConfig(PropBoneConfig source)
        {
            PropBoneConfig newPropBoneConfig = ScriptableObject.CreateInstance<PropBoneConfig>();
            newPropBoneConfig.propBoneDefinitions = source.propBoneDefinitions;
            newPropBoneConfig.sourceRig = source.sourceRig;
            newPropBoneConfig.targetRig = source.targetRig;
            return newPropBoneConfig;
        }

        /// <summary>
        ///     Creates a new PropBoneConfig containing the given PropBoneDefinitions.
        /// </summary>
        /// <param name="definitions">The prop bone definitions to be used by the new PropBoneConfig.</param>
        /// <returns>A <c>PropBoneConfig</c> with the given PropBoneDefinitions.</returns>
        private static PropBoneConfig CreatePropBoneConfig(PropBoneDefinition[] definitions)
        {
            PropBoneConfig newPropBoneConfig = ScriptableObject.CreateInstance<PropBoneConfig>();
            newPropBoneConfig.propBoneDefinitions = definitions;
            return newPropBoneConfig;
        }

        /// <summary>
        ///     Creates a new PropBoneConfigAsset of the given PropBoneConfig and with the given path.
        /// </summary>
        /// <param name="config">The config of the config asset.</param>
        /// <param name="path">The desired path for the config asset. The path used ma.</param>
        /// <param name="ensureUniquePath">When true the path is altered as necessary to ensure it is unique.</param>
        /// <returns>A new <c>PropBoneConfigAsset</c> based on the given parameters.</returns>
        private static PropBoneConfigAsset CreatePropBoneConfigAsset(PropBoneConfig config, string path, bool ensureUniquePath = true)
        {
            PropBoneConfigAsset result = new PropBoneConfigAsset();
            result.savedInAssetDatabase = false;
            result.path = path;
            if (ensureUniquePath)
            {
                result.path = AssetDatabase.GenerateUniqueAssetPath(result.path);
                if (string.IsNullOrEmpty(result.path))
                {
                    // AssetDatabase.GenerateUniqueAssetPath(result.path); returns an empty path if the folder structure in the file path does not exist.
                    result.path = path;
                }
            }
            result.config = config;
            return result;
        }

        /// <summary>
        ///     Saves the given PropBoneConfigAsset to the AssetDatabase.
        /// </summary>
        /// <param name="asset">The PropBoneConfigAsset to save.</param>
        private static void SavePropBoneAssetToProject(PropBoneConfigAsset asset)
        {
            EnsureFolderExists(asset.path);

            AssetDatabase.CreateAsset(asset.config, asset.path);
            AssetDatabase.SaveAssets();
            asset.savedInAssetDatabase = true;
            Debug.Log($"Successfully created prop bone config asset at path: {asset.path}.", asset.config);
        }

        /// <summary>
        ///     Checks if the folder at the given path exists, if not then the folder and all parent folders in the hierarchy are created.
        /// </summary>
        /// <param name="path">The folder path.</param>
        private static void EnsureFolderExists(string path)
        {
            string[] split = path.Split('/');
            string parentPath = split[0];
            for (int i = 1; i < split.Length - 1; ++i)
            {
                string head = split[i];
                if (!AssetDatabase.IsValidFolder(parentPath + "/" + head))
                {
                    AssetDatabase.CreateFolder(parentPath, head);
                }

                parentPath += "/" + head;
            }
        }

    }
}
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Fight;
using Fight.Number;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fight.EditorTools
{
    public static class MoveSpeedMigrationTool
    {
        private const int LegacySpeedMultiplierEnumValue = 2;
        private const int LegacyBaseMoveSpeed = 6;
        private const string MenuPath = "Tools/Fight/Migrate MoveSpeed Property";

        private struct MigrationStats
        {
            public int scannedPrefabs;
            public int scannedScenes;
            public int scannedEntities;
            public int modifiedAssets;
            public int modifiedEntities;
            public int conflictCount;
            public int errorCount;
        }

        [MenuItem(MenuPath)]
        public static void Migrate()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[MoveSpeedMigrationTool] 请在非运行模式下执行迁移。");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "MoveSpeed 迁移",
                    "将把旧 SpeedMultiplier 迁移为 MoveSpeed，并修改 Prefab/Scene 资源。是否继续？",
                    "继续",
                    "取消"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[MoveSpeedMigrationTool] 用户取消了场景保存，迁移已中止。");
                return;
            }

            var stats = new MigrationStats();
            var originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                MigratePrefabs(ref stats);
                MigrateScenes(ref stats);
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                stats.errorCount++;
                Debug.LogException(ex);
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
                AssetDatabase.Refresh();
            }

            Debug.Log(
                $"[MoveSpeedMigrationTool] 迁移完成。\n" +
                $"扫描 Prefab: {stats.scannedPrefabs}\n" +
                $"扫描 Scene: {stats.scannedScenes}\n" +
                $"扫描 CombatEntity: {stats.scannedEntities}\n" +
                $"修改资源数: {stats.modifiedAssets}\n" +
                $"修改实体数: {stats.modifiedEntities}\n" +
                $"冲突/异常配置数: {stats.conflictCount}\n" +
                $"错误数: {stats.errorCount}");
        }

        private static void MigratePrefabs(ref MigrationStats stats)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(assetPath);
                    var entities = root.GetComponentsInChildren<CombatEntity>(true);
                    if (entities.Length == 0)
                    {
                        continue;
                    }

                    stats.scannedPrefabs++;
                    bool changed = false;
                    foreach (var entity in entities)
                    {
                        changed |= MigrateCombatEntity(entity, assetPath, ref stats);
                    }

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                        stats.modifiedAssets++;
                    }
                }
                catch (Exception ex)
                {
                    stats.errorCount++;
                    Debug.LogError($"[MoveSpeedMigrationTool] Prefab 迁移失败: {assetPath}\n{ex}");
                }
                finally
                {
                    if (root != null)
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }
        }

        private static void MigrateScenes(ref MigrationStats stats)
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    stats.scannedScenes++;

                    bool changed = false;
                    var roots = scene.GetRootGameObjects();
                    foreach (var root in roots)
                    {
                        var entities = root.GetComponentsInChildren<CombatEntity>(true);
                        foreach (var entity in entities)
                        {
                            changed |= MigrateCombatEntity(entity, scenePath, ref stats);
                        }
                    }

                    if (changed)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        stats.modifiedAssets++;
                    }
                }
                catch (Exception ex)
                {
                    stats.errorCount++;
                    Debug.LogError($"[MoveSpeedMigrationTool] Scene 迁移失败: {scenePath}\n{ex}");
                }
            }
        }

        private static bool MigrateCombatEntity(CombatEntity entity, string assetPath, ref MigrationStats stats)
        {
            stats.scannedEntities++;

            var serializedObject = new SerializedObject(entity);
            SerializedProperty setupItems = serializedObject.FindProperty("propertySetupItems");
            if (setupItems == null || !setupItems.isArray)
            {
                stats.errorCount++;
                Debug.LogError($"[MoveSpeedMigrationTool] 无法找到 propertySetupItems: {assetPath}", entity);
                return false;
            }

            var legacyIndices = new List<int>();
            var moveSpeedIndices = new List<int>();

            for (int i = 0; i < setupItems.arraySize; i++)
            {
                var item = setupItems.GetArrayElementAtIndex(i);
                var typeProperty = item.FindPropertyRelative("propertyType");
                if (typeProperty == null)
                {
                    stats.errorCount++;
                    Debug.LogError($"[MoveSpeedMigrationTool] 缺少 propertyType 字段: {assetPath}", entity);
                    return false;
                }

                int propertyTypeValue = typeProperty.intValue;
                if (propertyTypeValue == LegacySpeedMultiplierEnumValue)
                {
                    legacyIndices.Add(i);
                }
                else if (propertyTypeValue == (int)PropertyType.MoveSpeed)
                {
                    moveSpeedIndices.Add(i);
                }
            }

            bool changed = false;

            if (moveSpeedIndices.Count > 1)
            {
                stats.conflictCount += moveSpeedIndices.Count - 1;
                for (int i = moveSpeedIndices.Count - 1; i >= 1; i--)
                {
                    setupItems.DeleteArrayElementAtIndex(moveSpeedIndices[i]);
                    changed = true;
                }
                moveSpeedIndices.RemoveRange(1, moveSpeedIndices.Count - 1);
            }

            if (legacyIndices.Count > 1)
            {
                stats.conflictCount += legacyIndices.Count - 1;
            }

            if (legacyIndices.Count > 0 && moveSpeedIndices.Count > 0)
            {
                stats.conflictCount++;
            }

            if (legacyIndices.Count > 0 && moveSpeedIndices.Count == 0)
            {
                int legacyBaseValue = GetBaseValue(setupItems.GetArrayElementAtIndex(legacyIndices[0]));
                int migratedMoveSpeed = Mathf.RoundToInt(LegacyBaseMoveSpeed * legacyBaseValue / 100f);
                AddPropertyItem(setupItems, (int)PropertyType.MoveSpeed, migratedMoveSpeed);
                changed = true;
            }
            else if (legacyIndices.Count == 0 && moveSpeedIndices.Count == 0)
            {
                AddPropertyItem(setupItems, (int)PropertyType.MoveSpeed, LegacyBaseMoveSpeed);
                changed = true;
            }

            if (legacyIndices.Count > 0)
            {
                for (int i = legacyIndices.Count - 1; i >= 0; i--)
                {
                    setupItems.DeleteArrayElementAtIndex(legacyIndices[i]);
                    changed = true;
                }
            }

            if (!changed)
            {
                return false;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(entity);
            stats.modifiedEntities++;
            return true;
        }

        private static int GetBaseValue(SerializedProperty item)
        {
            var baseValueProperty = item.FindPropertyRelative("baseValue");
            return baseValueProperty == null ? LegacyBaseMoveSpeed : baseValueProperty.intValue;
        }

        private static void AddPropertyItem(SerializedProperty setupItems, int propertyTypeValue, int baseValue)
        {
            int index = setupItems.arraySize;
            setupItems.InsertArrayElementAtIndex(index);
            var newItem = setupItems.GetArrayElementAtIndex(index);
            newItem.FindPropertyRelative("propertyType").intValue = propertyTypeValue;
            newItem.FindPropertyRelative("baseValue").intValue = Mathf.Max(0, baseValue);
        }
    }
}
#endif

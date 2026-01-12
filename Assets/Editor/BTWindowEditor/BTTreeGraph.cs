using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace BT.Editor
{
    [Graph(AssetExtension)]
    [Serializable]
    public class BTTreeGraph : Graph
    {
        public const string AssetExtension = "DefaultBTTreeGraph";

        [MenuItem("Assets/Create/EditorGraph/BT Tree Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BTTreeGraph>();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            RuntimeJson.BTGraphRootInitializer.EnsureRootNodeExists(this);
        }
    }
}

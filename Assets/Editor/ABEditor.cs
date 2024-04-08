using System.IO;
using UnityEditor;
using UnityEngine;
public class ABEditor
{
    /// <summary>
    /// AB包的输出路径
    /// </summary>
    public static string outputPath = Application.streamingAssetsPath + "/AssetBundles";

    [MenuItem("打包/生成ab包")]

    public static void BuildeAB()
    {
        if (!File.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        Debug.Log("AB包打包成功");
        AssetDatabase.Refresh();
    }
}

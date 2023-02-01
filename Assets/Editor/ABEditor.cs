using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ABEditor : MonoBehaviour
{
    /// <summary>
    /// 热更资源的根目录
    /// </summary>
    public static string rootpath = Application.dataPath + "/GAssets";
    /// <summary>
    /// AB包的输出路径
    /// </summary>
    public static string outputPath = Application.streamingAssetsPath;
    /// <summary>
    /// 一个ab包对应了一个build文件
    /// </summary>
    public static List<AssetBundleBuild> assetBundleList = new List<AssetBundleBuild>();
    /// <summary>
    /// 资源地址和对应的ab包
    /// </summary>
    public static Dictionary<string, string> Asset2Bundle = new Dictionary<string, string>();

    // public static List<string> assetnames = new List<string>();

    /// <summary>
    /// 某个ab包和所依赖的ab包
    /// </summary>
    public static Dictionary<string, List<string>> asset2Dependencies = new Dictionary<string, List<string>>();
    [MenuItem("AB包打包/生成ab包")]

    public static void BuildeAB()
    {
        if (!File.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();
    }
}

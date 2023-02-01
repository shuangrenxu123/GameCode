using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ABEditor : MonoBehaviour
{
    /// <summary>
    /// �ȸ���Դ�ĸ�Ŀ¼
    /// </summary>
    public static string rootpath = Application.dataPath + "/GAssets";
    /// <summary>
    /// AB�������·��
    /// </summary>
    public static string outputPath = Application.streamingAssetsPath;
    /// <summary>
    /// һ��ab����Ӧ��һ��build�ļ�
    /// </summary>
    public static List<AssetBundleBuild> assetBundleList = new List<AssetBundleBuild>();
    /// <summary>
    /// ��Դ��ַ�Ͷ�Ӧ��ab��
    /// </summary>
    public static Dictionary<string, string> Asset2Bundle = new Dictionary<string, string>();

    // public static List<string> assetnames = new List<string>();

    /// <summary>
    /// ĳ��ab������������ab��
    /// </summary>
    public static Dictionary<string, List<string>> asset2Dependencies = new Dictionary<string, List<string>>();
    [MenuItem("AB�����/����ab��")]

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

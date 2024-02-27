using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoadDll : MonoBehaviour
{
    //补充元数据dll的列表，
    //通过RuntimeApi.LoadMetadataForAOTAssembly()函数来补充AOT泛型的原始元数据
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
    };

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    private void Awake()
    {
        //GameObject go = new GameObject();
        //var cts = go.AddComponent<ConsoleToScreen>();
        //cts.fontSize = 20;
        //
        //DontDestroyOnLoad(go);
    }

    // Start is called before the first frame update
    void Start()
    {
        //做三件事
        //1.从资源服务器下载dll和入口AB包
        StartCoroutine(DownLoadAssets(this.StartGame));
    }


    public static byte[] GetAssetData(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private string GetWebRequestPath(string asset)
    {
        var path = $"http://1.14.67.47/dlls/{asset}";
        if (!path.Contains("://"))
        {
            path = "file://" + path;
        }
        if (path.EndsWith(".dll"))
        {
            path += ".bytes";
        }
        return path;
    }
    IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var DownloadAssets = new List<string>
        {
            "core",
            "scences",
            "Assembly-CSharp.dll",
        };

        foreach (var asset in AOTMetaAssemblyNames)
        {
            var path = Application.streamingAssetsPath + "/MateDlls/" + asset + ".bytes";
            if (!File.Exists(path))
            {
                DownloadAssets.Add(asset);
            }
            else
            {
                Debug.Log(asset + " 加载成功");
                using FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, bytes.Length);
                s_assetDatas[asset] = bytes;
            }
        }
        foreach (var asset in DownloadAssets)
        {
            string dllPath = GetWebRequestPath(asset);
            Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                byte[] assetData = www.downloadHandler.data;
                Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();
    }

    void StartGame()
    {
        //2.是为AOT dll补充元数据，加载热更新dll
        LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
        Assembly.Load(GetAssetData("Assembly-CSharp.dll"));
        Debug.Log("加载热更DLL成功");
#endif

        //3.实例化HotUpdateMain.prefab，通过HotUpdateMain.cs执行旧工程入口代码
        AssetBundle scences = AssetBundle.LoadFromMemory(GetAssetData("scences"));
        AssetBundle prefabAb = AssetBundle.LoadFromMemory(GetAssetData("core"));
        SceneManager.LoadScene("CharacterControlTest");
        GameObject gm = Instantiate(prefabAb.LoadAsset<GameObject>("GameManager.prefab"));
        GameObject.DontDestroyOnLoad(gm);
        //GameObject canves = Instantiate(prefabAb.LoadAsset<GameObject>("Canvas.prefab"));
    }

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }

}

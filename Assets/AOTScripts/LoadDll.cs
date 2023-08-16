using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour
{
    //����Ԫ����dll���б���
    //ͨ��RuntimeApi.LoadMetadataForAOTAssembly()����������AOT���͵�ԭʼԪ����
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
    };

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    private void Awake()
    {
        GameObject go = new GameObject();
        var cts = go.AddComponent<ConsoleToScreen>();
        cts.fontSize = 20;

        DontDestroyOnLoad(go);
    }

    // Start is called before the first frame update
    void Start()
    {
        //��������
        //1.����Դ����������dll�����AB��
        StartCoroutine(DownLoadAssets(this.StartGame));
    }


    public static byte[] GetAssetData(string dllName)
    {
        return s_assetDatas[dllName];
    }

    private string GetWebRequestPath(string asset)
    {
        //var path = $"{Application.streamingAssetsPath}/{asset}";
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
        var assets = new List<string>
        {
            "core",
            "Assembly-CSharp.dll",
        };

        foreach (var asset in AOTMetaAssemblyNames)
        {
            var path = Application.streamingAssetsPath + "/MateDlls/" + asset+".bytes";
            if (!File.Exists(path))
            {
                assets.Add(asset);
            }
            else
            {
                Debug.Log(asset+" ���سɹ�");
                using FileStream file = new FileStream(path,FileMode.Open,FileAccess.Read);
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, bytes.Length);
                s_assetDatas[asset] = bytes;
            }
        }
        foreach (var asset in assets)
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
        //2.��ΪAOT dll����Ԫ���ݣ������ȸ���dll
        LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
        System.Reflection.Assembly.Load(GetAssetData("Assembly-CSharp.dll"));
#endif

        //3.ʵ����HotUpdateMain.prefab��ͨ��HotUpdateMain.csִ�оɹ�����ڴ���
        AssetBundle prefabAb = AssetBundle.LoadFromMemory(GetAssetData("core"));
        GameObject gm = Instantiate(prefabAb.LoadAsset<GameObject>("GameManager.prefab"));
        GameObject canves = Instantiate(prefabAb.LoadAsset<GameObject>("Canvas.prefab"));
    }

    /// <summary>
    /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllName);
            // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }

}
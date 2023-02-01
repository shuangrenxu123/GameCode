using LitJson;
using System.IO;
using UnityEngine;

public class VersionManager : ModuleSingleton<VersionManager>, IModule
{
    public string appVersion;
    public string resVersion;
    public string cacheResVersionFile { get { return Application.persistentDataPath + "/update/version.txt"; } }

    public void OnCreate(object createParam)
    {
        //��ð汾��
        string version = Resources.Load<TextAsset>("version").text;
        //json�а����˰汾�ź���Դ�汾��
        var jsonData = JsonMapper.ToObject(version);
        appVersion = jsonData["app_version"].ToString();
        resVersion = jsonData["res_version"].ToString();
        var cacheVersion = ReadCacheResVersion();
        if (CompareVersion(cacheVersion, resVersion) > 0)
        {
            resVersion = cacheVersion;
        }

    }
    ///<summary>
    ///��û���汾��
    /// </summary>
    public string ReadCacheResVersion()
    {
        if (File.Exists(cacheResVersionFile))
        {
            using (var f = File.OpenRead(cacheResVersionFile))
            {
                using (var sr = new StreamReader(f))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        return "0.0.0.0";
    }
    /// <summary>
    /// ���»������Դ�汾�ţ�ͬʱ��д�뵽�ļ���
    /// </summary>
    /// <param name="newVersion"></param>
    public void UpdateResVersion(string newVersion)
    {
        resVersion = newVersion;
        //�п�������û��update�ļ��С�������Ҫ����һ��
        var dir = Path.GetDirectoryName(cacheResVersionFile);
        if (Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        using (var f = File.OpenWrite(cacheResVersionFile))
        {
            using (var sw = new StreamWriter(f))
            {
                sw.Write(resVersion);
            }
        }
    }
    /// <summary>
    /// ɾ������汾���ļ�
    /// </summary>
    public void DeleteResVersion()
    {
        if (File.Exists(cacheResVersionFile))
        {
            File.Delete(cacheResVersionFile);
        }
    }
    /// <summary>
    /// �Ƚ������汾�ţ���v1����v2�򷵻� 1����֮����-1���������Ϊ0
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public int CompareVersion(string v1, string v2)
    {
        if (v1 == v2)
            return 0;
        string[] v1Array = v1.Split('.');
        string[] v2Array = v2.Split('.');
        for (int i = 0; i < v1.Length; i++)
        {
            if (int.Parse(v1Array[i]) > int.Parse(v2Array[i]))
            {
                return 1;
            }
        }
        return -1;
    }
    public void OnUpdate()
    {

    }

}

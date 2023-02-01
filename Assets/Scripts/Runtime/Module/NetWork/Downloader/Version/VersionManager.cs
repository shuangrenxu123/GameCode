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
        //获得版本号
        string version = Resources.Load<TextAsset>("version").text;
        //json中包含了版本号和资源版本号
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
    ///获得缓存版本号
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
    /// 更新缓存的资源版本号，同时会写入到文件中
    /// </summary>
    /// <param name="newVersion"></param>
    public void UpdateResVersion(string newVersion)
    {
        resVersion = newVersion;
        //有可能我们没有update文件夹。所以需要创建一个
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
    /// 删除缓存版本号文件
    /// </summary>
    public void DeleteResVersion()
    {
        if (File.Exists(cacheResVersionFile))
        {
            File.Delete(cacheResVersionFile);
        }
    }
    /// <summary>
    /// 比较两个版本号，若v1大于v2则返回 1，反之返回-1，若相等则为0
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

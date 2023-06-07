using LitJson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetConfig
{
    /// <summary>
    /// id  
    /// 具体的数据
    /// </summary>
    protected readonly Dictionary<string, JsonData> tables = new Dictionary<string, JsonData>();
    private JsonData jsondata;
    public bool IsLoad = false;
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="loaction">资源的地址</param>
    public void Load(string loaction)
    {
        jsondata = JsonMapper.ToObject(File.ReadAllText(loaction));
        IsLoad = true;
    }

    private void LoadTest(string json)
    {
        jsondata = JsonMapper.ToObject(json);
        foreach (JsonData i in jsondata)
        {
            //Debug.Log(i["id"].ToString());
            tables.Add(i["id"].ToString(), i);
        }
    }
    public void LoadAllTable<T>(string loaction)
    {
        Load(loaction);
        foreach (JsonData i in jsondata)
        {
            tables.Add(i["id"].ToString(), i);
        }
    }

    public string GetValue(string name)
    {
        return jsondata[name][0].ToJson();
    }
    public T GetTable<T>(string id)
    {
        if (tables.ContainsKey(id))
            return JsonMapper.ToObject<T>(tables[id].ToJson().ToString());
        else
        {
            AddTable(id);
            return GetTable<T>(id);
        }
    }
    public void AddTable(string id)
    {
        if (jsondata != null)
        {
            foreach (JsonData i in jsondata)
            {
                if (i["id"].ToString() == id)
                    tables.Add(id, i);
                return;
            }
        }
        else
            Debug.LogError("id标记的数据不存在");
    }
}

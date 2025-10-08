using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 单个AI的数据存储。
/// </summary>
public class DataBase<TKey, TValue>
{
    Dictionary<TKey, TValue> data = new();
    public T GetData<T>(TKey dataName)
    {
        if (!data.ContainsKey(dataName))
        {
            Debug.LogError($"没有在黑板中找到相关数据 {dataName} ");
            return default;
        }
        return (T)(object)data[dataName];
    }
    public void SetData<T>(TKey dataName, T data)
    {
        this.data[dataName] = (TValue)(object)data;
    }
    public bool CheckDataNull(TKey dataName)
    {
        return !data.ContainsKey(dataName) || data[dataName] == null || data[dataName].Equals(null);
    }
    public bool ContainsData(TKey dataName)
    {
        return data.ContainsKey(dataName);
    }

    public void Reset()
    {
        data.Clear();
    }
}
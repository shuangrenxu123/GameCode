using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 单个AI的数据存储。
/// </summary>
public class DataBase<TKey, TValue>
{
    private Dictionary<TKey, TValue> _data = new Dictionary<TKey, TValue>();
    public T GetData<T>(TKey dataName)
    {
        if (!_data.ContainsKey(dataName))
        {
            Debug.LogError("没有在黑板中找到相关数据");
            return default;
        }
        return (T)(object)_data[dataName];
    }
    public void SetData<T>(TKey dataName, T data)
    {
        _data[dataName] = (TValue)(object) data;
    }
    public bool CheckDataNull(TKey dataName)
    {
        return !_data.ContainsKey(dataName) || _data[dataName] == null || _data[dataName].Equals(null);
    }
    public bool ContainsData(TKey dataName)
    {
        return _data.ContainsKey(dataName);
    }
}
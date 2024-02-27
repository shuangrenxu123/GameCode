using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Config
{
    public class AssetConfig
    {
        /// <summary>
        /// id  
        /// ���������
        /// </summary>
        protected readonly Dictionary<int, object> Datas = new Dictionary<int, object>();
        public T GetData<T>(int id) where T : class
        {
            if (Datas.ContainsKey(id))
                return Datas[id] as T;
            else
            {
                Debug.LogError("������ʱ���� ��" + id);
                return default;
            }
        }
        public void AddData<T>(int id, T obj)
        {
            Datas.Add(id, obj);
        }
        public T[] GetAllData<T>()
        {
            return Datas.Values.ToArray() as T[];
        }
        public bool ContainsKey(int key)
        {
            return Datas.ContainsKey(key);
        }
        /// <summary>
        /// ��ȡ����Key
        /// </summary>
        public List<int> GetKeys()
        {
            List<int> keys = new List<int>(Datas.Count);
            foreach (var tab in Datas)
            {
                keys.Add(tab.Key);
            }
            return keys;
        }
    }
}
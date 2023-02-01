using System.Collections.Generic;
using UnityEngine;
namespace BT
{
    public class BTDataBase : MonoBehaviour
    {
        private List<object> _database = new List<object>();
        private List<string> _dataNames = new List<string>();
        public T GetData<T>(string dataName)
        {
            int dataId = IndexOfDataId(dataName);
            if (dataId == -1) Debug.LogError("Database: Data for " + dataName + " does not exist!");

            return (T)_database[dataId];
        }

        // Should use this function to get data!
        public T GetData<T>(int dataId)
        {
            return (T)_database[dataId];
        }

        public void SetData<T>(string dataName, T data)
        {
            int dataId = GetDataId(dataName);
            _database[dataId] = (object)data;
        }

        public void SetData<T>(int dataId, T data)
        {
            _database[dataId] = (object)data;
        }

        public int GetDataId(string dataName)
        {
            int dataId = IndexOfDataId(dataName);
            if (dataId == -1)
            {
                _dataNames.Add(dataName);
                _database.Add(null);
                dataId = _dataNames.Count - 1;
            }

            return dataId;
        }

        private int IndexOfDataId(string dataName)
        {
            for (int i = 0; i < _dataNames.Count; i++)
            {
                if (_dataNames[i].Equals(dataName)) return i;
            }

            return -1;
        }

        public bool ContainsData(string dataName)
        {
            return IndexOfDataId(dataName) != -1;
        }
    }
}
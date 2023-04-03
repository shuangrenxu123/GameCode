using System.Collections.Generic;
using UnityEngine;
namespace BT
{
    public class BTDataBase : MonoBehaviour
    {
        private List<object> _dataList = new List<object>();
        private List<string> _dataNames = new List<string>();
        public T GetData<T>(string dataName)
        {
            int dataId = IndexOfDataId(dataName);
            if (dataId == -1) Debug.LogError("Database: Data for " + dataName + " does not exist!");

            return (T)_dataList[dataId];
        }
        public T GetData<T>(int dataId)
        {
            return (T)_dataList[dataId];
        }

        public void SetData<T>(string dataName, T data)
        {
            int dataId = GetDataId(dataName);
            _dataList[dataId] = (object)data;
        }

        public void SetData<T>(int dataId, T data)
        {
            _dataList[dataId] = (object)data;
        }


        public bool CheckDataNull(string dataName)
        {
            int dataId = IndexOfDataId(dataName);
            if (dataId == -1) return true;

            return CheckDataNull(dataId);
        }
        public bool CheckDataNull(int dataId)
        {
            return _dataList[dataId] == null || _dataList[dataId].Equals(null);
        }
        public int GetDataId(string dataName)
        {
            int dataId = IndexOfDataId(dataName);
            if (dataId == -1)
            {
                _dataNames.Add(dataName);
                _dataList.Add(null);
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
using LitJson;
using UnityEngine;

namespace GameSave
{
    public class GameFileData : IGameSave
    {
        public DataType dataType => DataType.FileData;
        public GameFileData()
        {
            GameSaveManager.Instance.RegisterSaver(this);
        }
        class Data
        {
            public string currentTime;
            public Data()
            {
                currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public void LoadData(string json)
        {
            var data = JsonMapper.ToObject<Data>(json);
            Debug.Log(data.currentTime);
        }

        public string SaveData()
        {
            return JsonMapper.ToJson(new Data());
        }
    }
}

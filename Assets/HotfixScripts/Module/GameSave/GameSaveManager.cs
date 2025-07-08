using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
namespace GameSave
{

    public class GameSaveManager : ModuleSingleton<GameSaveManager>, IModule
    {
        string savePath = Application.persistentDataPath;
        Dictionary<DataType, IGameSave> saveDataEntities = new();
        Dictionary<string, string> saveData = new();

        string currentFileName;

        GameFileData fileData;

        public void OnCreate(object createParam)
        {
            fileData = new();
        }

        public void RegisterSaver(IGameSave saver)
        {
            if (saveDataEntities.ContainsKey(saver.dataType))
            {
                throw new Exception("该数据保存者已经存在");
            }

            saveDataEntities.Add(saver.dataType, saver);
        }

        public void SaveData(string fileName)
        {
            saveData.Clear();
            foreach (var save in saveDataEntities)
            {
                var temp = save.Value.SaveData();
                saveData.Add(save.Key.ToString(), temp);
            }

            var json = JsonMapper.ToJson(saveData);
            File.WriteAllText(Path.Combine(savePath, $"{fileName}.saveData"), json);
            Debug.Log("Save Success");
        }

        public void LoadDataFile(string fileName)
        {
            currentFileName = fileName;

            var path = Path.Combine(savePath, $"{fileName}.saveData");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"{fileName} Not Found");
            }

            var data = File.ReadAllText(path);
            var json = JsonMapper.ToObject<Dictionary<string, string>>(data);

            foreach (var JObject in json)
            {
                var type = Enum.Parse<DataType>(JObject.Key);
                saveDataEntities[type].LoadData(JObject.Value);
            }

            Debug.Log("Load Success");
        }

        public void SaveValue(string key, string value)
        {

        }

        public string LoadValue(string key, string defaultValue)
        {
            return defaultValue;
        }

        public void OnUpdate()
        {
        }
    }
}

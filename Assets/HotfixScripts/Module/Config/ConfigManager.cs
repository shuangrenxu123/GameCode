using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utf8Json;
namespace Config
{
    public class ConfigManager : ModuleSingleton<ConfigManager>, IModule
    {
        /// <summary>
        /// ��Ӧ�������ļ� �������
        /// </summary>
        private Dictionary<string, AssetConfig> assetConfigs = new Dictionary<string, AssetConfig>();
        private string path = Application.streamingAssetsPath + "/config/";
        public void OnCreate(object createParam)
        {
        }
        public void OnUpdate()
        {

        }

        public void LoadConfig<T>(string name, string location) where T : class
        {
            var path = this.path + location;
            if (assetConfigs.ContainsKey(name))
                return;
            else
            {
                using StreamReader sr = new StreamReader(path);
                var json = JsonSerializer.Deserialize<dynamic>(sr.ReadToEnd());
                foreach (var data in json["members"])
                {
                    //Debug.Log(data["name"]);
                    assetConfigs[name].AddData((int)data["id"], JsonSerializer.Deserialize<T>(data.ToJson()));

                }
            }
        }
        public AssetConfig GetAssetConfig(string name)
        {
            if (assetConfigs.ContainsKey(name))
                return assetConfigs[name];
            else
                return null;
        }
    }
}
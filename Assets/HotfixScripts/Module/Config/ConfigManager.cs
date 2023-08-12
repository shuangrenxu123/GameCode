using System.Collections.Generic;

public class ConfigManager : ModuleSingleton<ConfigManager>, IModule
{
    /// <summary>
    /// 对应的配置文件 和其对象
    /// </summary>
    private Dictionary<string, AssetConfig> assetConfigs = new Dictionary<string, AssetConfig>();
    public void OnCreate(object createParam)
    {

    }
    public void OnUpdate()
    {

    }

    public AssetConfig LoadTest(string name, string json)
    {
        AssetConfig assetConfig = new AssetConfig();
        assetConfigs.Add(name, assetConfig);
        return assetConfig;
    }

    public AssetConfig LoadConfig(string name, string location)
    {
        AssetConfig config = new AssetConfig();

        if (assetConfigs.ContainsKey(name))
            return assetConfigs[name];
        else
        {
            config.Load(location);
            assetConfigs.Add(name, config);
        }
        return config;
    }
    public AssetConfig GetAssetConfig(string name)
    {
        if (assetConfigs.ContainsKey(name))
            return assetConfigs[name];
        else
            return null;
    }


}

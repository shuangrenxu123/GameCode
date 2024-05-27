using Audio;
using Config;
using Downloader;
using Network;
using ObjectPool;
using Assets;
using UIWindow;
using UnityEngine;
public class Main : MonoBehaviour
{
    private void Awake()
    {
        //参数设置
        var a = new NetWorkManager.CreateParameters();
        a.PackageCoderType = typeof(DefaultNetworkPackageCoder);
        a.PackageBodyCoderType = typeof(ProtobufCoder);

        Engine.CreateModule<EventManager>();
        Engine.CreateModule<UIManager>();
        Engine.CreateModule<ResourcesManager>();
        Engine.CreateModule<PoolManager>();
        Engine.CreateModule<ReferenceManager>();
        Engine.CreateModule<VersionManager>();
        Engine.CreateModule<ConfigManager>();
        Engine.CreateModule<AudioManager>();
        Engine.CreateModule<NetWorkManager>(a);
        Engine.CreateModule<LogManager>();

    }
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        DontDestroyOnLoad(this);
    }
    private void OnApplicationQuit()
    {
        LogManager.Instance.Dispose();
    }
    private void Update()
    {
        Engine.Update();
    }
}
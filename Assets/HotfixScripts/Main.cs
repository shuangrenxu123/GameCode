using Audio;
using Config;
using Network;
using ObjectPool;
using UnityEngine;
public class Main : MonoBehaviour
{
    private void Awake()
    {
        //参数设置
        var a = new NetWorkManager.CreateParameters();
        a.PackageCoderType = typeof(DefaultNetworkPackageCoder);
        a.PackageBodyCoderType = typeof(ProtobufCoder);

        MotionEngine.CreateModule<NetWorkManager>(a);
        MotionEngine.CreateModule<EventManager>();
        MotionEngine.CreateModule<UIManager>();
        MotionEngine.CreateModule<ResourcesManager>();
        MotionEngine.CreateModule<PoolManager>();
        MotionEngine.CreateModule<VersionManager>();
        MotionEngine.CreateModule<ConfigManager>();
        MotionEngine.CreateModule<AudioManager>();

    }
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        MotionEngine.Update();
    }
}
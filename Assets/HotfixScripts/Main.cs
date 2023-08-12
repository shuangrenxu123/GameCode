using NetWork;
using UnityEngine;

public class Main : MonoBehaviour
{
    private void Awake()
    {
        //参数设置
        var a = new NetWorkManager.CreateParameters();
        a.PackageCoderType = typeof(DefaultNetworkPackageCoder);
        MotionEngine.CreateModule<NetWorkManager>(a);
        MotionEngine.CreateModule<EventManager>();
        MotionEngine.CreateModule<WindowsManager>();
        MotionEngine.CreateModule<AudioManager>();
        MotionEngine.CreateModule<ResourcesManager>();
        MotionEngine.CreateModule<PoolManager>();
        MotionEngine.CreateModule<VersionManager>();
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
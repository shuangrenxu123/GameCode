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
        MotionEngine.CreateModule<ResourcesManager>();
        MotionEngine.CreateModule<PoolManager>();
        MotionEngine.CreateModule<VersionManager>();
        MotionEngine.CreateModule<ConfigManager>();
        MotionEngine.CreateModule<AudioManager>();

        Debug.Log("22222");
        Debug.Log(4444);
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
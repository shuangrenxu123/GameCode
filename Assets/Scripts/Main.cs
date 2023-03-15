using NetWork;
using UnityEngine;

public class Main : MonoBehaviour
{
    //public IMethod update;
    public bool updateOver = false;
    private void Awake()
    {
        //MotionEngine.Initialize(this);
        //参数设置
        var a = new NetWorkManager.CreateParameters();
        a.PackageCoderType = typeof(DefaultNetworkPackageCoder);
        MotionEngine.CreateModule<NetWorkManager>(a);
        //MotionEngine.CreateModule<EventManager>();
        MotionEngine.CreateModule<WindowsManager>();
        //MotionEngine.CreateModule<VersionManager>();
        //MotionEngine.CreateModule<PoolManager>();
        //MotionEngine.CreateModule<ProcessManager>();
    }
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        //if (updateOver)
        //{
        //    //ILRuntimeHelp.appDomain.Invoke("Hotfix.HotfixMain", "update", null,null);
        //    ILRuntimeHelp.appDomain.Invoke(update, null, null);
        //}
        MotionEngine.Update();
    }
}

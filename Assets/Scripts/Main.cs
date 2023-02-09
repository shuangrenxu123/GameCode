//using ILRuntime.CLR.Method;
using NetWork;
using PlayerInfo;
using UnityEngine;
using UnityEngine.UI;

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
        //MotionEngine.CreateModule<WindowsManager>();
        //MotionEngine.CreateModule<VersionManager>();
        //MotionEngine.CreateModule<PoolManager>();
        //MotionEngine.CreateModule<ProcessManager>();
    }
    private void Start()
    {
        DontDestroyOnLoad(this);
        NetWorkManager.Instance.ConnectServer("192.168.31.143", 9990);
       // GameObject.Find("Canvas/Button").GetComponent<Button>().onClick.AddListener(
       //     () => NetWorkManager.Instance.SendMessage(1, new login(){ Id = 2, Pw = "222" }));
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

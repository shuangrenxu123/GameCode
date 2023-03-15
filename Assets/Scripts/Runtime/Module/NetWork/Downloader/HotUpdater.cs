using Ionic.Zip;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
public class HotUpdater
{
    private Dictionary<Thread, Downloader> runingList = new Dictionary<Thread, Downloader>();
    private Queue<PackInfo> readyList = new Queue<PackInfo>();
    private object locker = new object();
    const int MAX_THREAD_COUNT = 20;
    private DownloadingInfo downloadingInfo;
    /// <summary>
    /// 没有任何更新
    /// </summary>
    public Action actionNothongUpdate;
    /// <summary>
    /// 所有包下载完毕以后
    /// </summary>
    public Action actionAllDownloadDone;
    /// <summary>
    /// 进度条函数更新
    /// </summary>
    public Action<float> actionDownloadValue;
    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool hasNextUpdateBtn;
    /// <summary>
    /// 更新的URL,即服务器地址
    /// </summary>
    string fullAppUpdateUrl;
    /// <summary>
    /// 相同版本号的资源更新
    /// </summary>
    bool sameAppVerResUpdate;
    string bigestVer;
    private Downloader downloader;
    private IEnumerator OnDownloadItr;
    private async void Init()
    {
        // 解决HTTPS证书问题
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        if (!CheckNetWorkActive())
            Debug.LogError("网络错误，没有连接网络");
        hasNextUpdateBtn = false;
        fullAppUpdateUrl = "http://1.116.50.215";
        sameAppVerResUpdate = false;
        actionAllDownloadDone += UpdateAppversion;
        downloadingInfo = new DownloadingInfo();
        var updateInfos = await ReqUpdateInfo();
        if (updateInfos != null)
        {
            SortUpdateInfo(ref updateInfos);
            //为后面的更新类型做铺垫
            CalculateUpdatePackList(updateInfos);
            if (!sameAppVerResUpdate)
            {
                Debug.Log("没有任何更新");
                actionNothongUpdate?.Invoke();
            }
        }
        else
        {
            Debug.LogError("没有获取到版本列表");
        }
    }
    #region 多线程下载相关
    /// <summary>
    /// 多线程开始下载资源
    /// </summary>
    public void DownloadAsync()
    {
        Init();
        if (sameAppVerResUpdate)
        {
            CreateThread();
        }
    }
    private void CreateThread()
    {
        for (int i = 0; i < MAX_THREAD_COUNT; i++)
        {
            var thread = new Thread(ThreadLoop);
            runingList.Add(thread, new Downloader());
            thread.Start();
        }
    }
    private void ThreadLoop()
    {
        var downloader = runingList[Thread.CurrentThread];
        while (true)
        {
            //如果正在下载就直接跳过
            if (downloader.state == Downloader.DownloadState.Ing)
                continue;
            //如果是结束或者就绪状态就下载
            if (readyList.Count > 0)
            {
                lock (locker)
                {
                    if (downloader.state == Downloader.DownloadState.End || downloader.state == Downloader.DownloadState.Ready)
                    {
                        var info = readyList.Dequeue();
                        runingList[Thread.CurrentThread].Start(info);
                    }
                }
            }
            //当不需要下载了以后就会停止该线程，最终所有的线程都会慢慢停止
            else
            {
                break;
            }
            //如果是网络相关问题就直接重新下载
            if (downloader.state == Downloader.DownloadState.DataProcessingError || downloader.state == Downloader.DownloadState.ConnectionError)
            {
                lock (locker)
                {
                    readyList.Enqueue(downloader.packInfo);
                }
            }
            //如果是其他问题就直接结束线程
            else
            {
                break;
            }
        }
    }
    private void UpdateThread()
    {
        //关闭一些有问题的线程，并将他们的信息重新添加到等待队列
        lock (locker)
        {
            List<Thread> threads = new List<Thread>();
            foreach (var i in runingList)
            {
                if (i.Key.IsAlive)
                    continue;
                if (i.Value != null)
                    readyList.Enqueue(i.Value.packInfo);
                threads.Add(i.Key);
            }
            foreach (var thread in threads)
            {
                runingList.Remove(thread);
                //强制终止线程
                thread.Abort();
            }
        }
        if (readyList.Count == 0 && runingList.Count == 0)
        {
            actionAllDownloadDone?.Invoke();
            return;
        }

        if (!CheckNetWorkActive())
        {
            Debug.LogError("没有网络了");
            return;
        }

        if (runingList.Count >= MAX_THREAD_COUNT)
            return;
        //如果还有其他的任务未完成，那就补充线程数量
        else if (readyList.Count > 0)
        {
            var thread = new Thread(ThreadLoop);
            lock (locker)
            {
                runingList.Add(thread, new Downloader());
            }
            thread.Start();
        }
    }
    /// <summary>
    /// 多线程下载的update
    /// </summary>
    public void UpdateAsync()
    {
        UpdateThread();
    }
    #endregion
    #region 单线程下载的版本
    public void Download()
    {
        Init();
        if (sameAppVerResUpdate)
        {
            StartDownloadResPack();
        }
    }
    /// <summary>
    /// 单线程下的下载更新 需要写在主线程中
    /// </summary>
    public void Update()
    {
        if (downloader != null)
        {
            switch (downloader.state)
            {
                case Downloader.DownloadState.ConnectionError:
                    Debug.LogError("下载错误");
                    break;
                case Downloader.DownloadState.DataProcessingError:
                    Debug.LogError("下载错误");
                    break;
                case Downloader.DownloadState.Ing:
                    OnDownloading();
                    break;
                case Downloader.DownloadState.End:
                    downloader = null;
                    OnDownloadItr = OnDownloadEnd();
                    break;
                default:
                    break;
            }

        }
        //开启下一个下载，即解压已经下载好了的压缩包，然后开始判断下一个下载
        RunCorotine();
    }
    private void RunCorotine()
    {
        //movenext会启动携程
        if (OnDownloadItr != null && !OnDownloadItr.MoveNext())
        {
            OnDownloadItr = null;
        }
    }
    /// <summary>
    /// 一个包下载完毕以后
    /// </summary>
    private IEnumerator OnDownloadEnd()
    {
        var MD5 = CheckMD5();
        if (!MD5)
        {
            //如果下载的不正确，那么就会重新下载
            var info = readyList.Peek();
            var ErrorFile = Application.persistentDataPath + "/" + info.MD5;
            DeleteFile(ErrorFile);
            downloader = null;
            downloader = new Downloader();
            downloader.Start(info);
        }
        //如果成功了则取出
        var packInfo = readyList.Dequeue();
        var file = Application.persistentDataPath + "/" + packInfo.MD5;
        yield return null;
        int index = 0;
        using (ZipFile zip = new(file))
        {
            var count = zip.Count;
            foreach (var i in zip)
            {
                index++;
                i.Extract(Application.persistentDataPath + "/update/", ExtractExistingFileAction.OverwriteSilently);
                yield return null;
            }
        }
        //删除临时文件
        DeleteFile(file);

        actionDownloadValue?.Invoke(1);
        StartDownloadResPack();
    }
    ///<summary>
    ///下载中的时候来更新进度条
    ///</summary>
    public void OnDownloading()
    {
        //获得当前下载了的文件
        downloadingInfo.currDownloadsize = downloader.currDownloadSize;
        float value = (float)downloadingInfo.currDownloadsize / downloadingInfo.targetDownloadSize;
        actionDownloadValue?.Invoke(value);

    }
    /// <summary>
    /// 开始下载资源包
    /// </summary>
    public void StartDownloadResPack()
    {
        if (readyList.Count == 0)
        {
            Debug.Log("所有包下载完毕");
            actionAllDownloadDone?.Invoke();
            downloader.Reset();
            downloader = null;
            return;
        }
        //这里仅仅是获得包，但是不会取出
        var packinfo = readyList.Peek();
        downloadingInfo.targetDownloadSize = packinfo.size;
        downloadingInfo.totalPackCount = readyList.Count;
        downloader = new Downloader();
        downloader.Start(packinfo);

    }
    #endregion
    #region 版本相关
    private void UpdateAppversion()
    {
        VersionManager.Instance.UpdateAppVersion(bigestVer);
    }
    private void DeleteFile(string file)
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
    /// <summary>
    /// 将列表中的按逆序来排序
    /// </summary>
    /// <param name="updateInfos"></param>
    private void SortUpdateInfo(ref List<UpdateInfo> updateInfos)
    {
        updateInfos.Sort((a, b) =>
        {
            return VersionManager.Instance.CompareVersion(b.appVersion, a.appVersion);
        });
    }
    private bool CheckMD5()
    {
        return true;
    }
    /// <summary>
    /// 请求更新列表，就是下载版本列表
    /// </summary>
    /// <returns></returns>
    private async Task<List<UpdateInfo>> ReqUpdateInfo()
    {
        UnityWebRequest uwr = UnityWebRequest.Get(fullAppUpdateUrl + "/update_list.json");
        var request = uwr.SendWebRequest();
        while (!request.isDone)
        {
            await Task.Delay(1);
        }
        //卡线程等待返回结果
        if (uwr.error != null)
        {
            Debug.LogError("没有收到更新列表");
            actionNothongUpdate.Invoke();
            return null;
        }
        return JsonMapper.ToObject<List<UpdateInfo>>(uwr.downloadHandler.text);
    }
    /// <summary>
    /// 分析更新的类型,同时将需要下载的包加入到队列中
    /// </summary>
    /// <param name="updateinfos"></param>
    private void CalculateUpdatePackList(List<UpdateInfo> updateinfos)
    {
        //获得当前的版本
        bigestVer = VersionManager.Instance.appVersion;
        List<PackInfo> packList = new List<PackInfo>();
        foreach (UpdateInfo info in updateinfos)
        {
            ///需要注意版本号应该从小往大来加载，不然会出现漏掉的情况
            if (VersionManager.Instance.CompareVersion(info.appVersion, bigestVer) > 0)
            {
                sameAppVerResUpdate = true;
                //更新版本号
                bigestVer = info.appVersion;
                packList.AddRange(info.updateList);
            }
        }
        readyList = new Queue<PackInfo>(packList);
    }
    /// <summary>
    /// 解决HTTPs问题
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="certificate"></param>
    /// <param name="chain"></param>
    /// <param name="sslPolicyErrors"></param>
    /// <returns></returns>
    private bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None) return true;
        bool isOk = true;
        for (int i = 0; i < chain.ChainStatus.Length; i++)
        {
            if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
            {
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }
    /// <summary>
    /// 检测网络类型
    /// </summary>
    /// <returns></returns>
    public bool CheckNetWorkActive()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            return true;
        return false;
    }
    #endregion
}
//单线程下下载的信息
class DownloadingInfo
{
    public long currDownloadsize;
    public long targetDownloadSize;
    public int currPackIndex;
    public int totalPackCount;
}
//版本号信息
public class UpdateInfo
{
    //版本的版本号
    public string appVersion;
    //版本下载地址
    public string appUrl;
    //版本所包含的资源包
    public List<PackInfo> updateList = new List<PackInfo>();
}
//每个文件的信息
public class PackInfo
{
    /// <summary>
    /// 包对应的MD5
    /// </summary>
    public string MD5;
    /// <summary>
    /// 包的大小
    /// </summary>
    public int size;
    /// <summary>
    /// 包对应的下载地址
    /// </summary>
    public string url;
}
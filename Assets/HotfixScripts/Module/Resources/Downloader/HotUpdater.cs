using Ionic.Zip;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
public class HotUpdater
{
    private Queue<PackInfo> readyList = new();
    private List<FileDownloader> downloaders = new(5);
    //下载器是否空闲
    private List<bool> downloadstates = new List<bool>(10);

    private int downloadThreads = 5;
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
    /// 更新的URL,即服务器地址
    /// </summary>
    string fullAppUpdateUrl;
    string bigestVer;

    private bool needUpdate = false;

    private async Task Init()
    {
        // 解决HTTPS证书问题
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

        if (!CheckNetWorkActive())
            Debug.LogError("网络错误，没有连接网络");

        fullAppUpdateUrl = PathUtil.UpdateURL;
        actionAllDownloadDone += UpdateAppversion;
        List<UpdateInfo> updateInfos = await ReqUpdateInfo();
        if (updateInfos != null)
        {
            //获得更新表
            SortUpdateInfo(ref updateInfos);
            //判断是否需要更新
            CalculateUpdatePackList(updateInfos);
        }
        else
        {
            Debug.LogError("没有获取到版本列表");
        }

    }
    public async void Download()
    {
        await Init();

        //更新下载池
        for (int i = 0; i < downloadThreads; i++)
        {
            downloaders.Add(new FileDownloader(DownloadedCallBack, DownloadFaildCallBack, true));
            downloadstates.Add(true);
        }

        if (needUpdate && readyList.Count > 0)
        {
            StartDownloadResPack();
        }
        else
        {
            Debug.Log("不需要更新");
            actionNothongUpdate?.Invoke();
        }
    }
    private void DownloadFaildCallBack(FileDownloader downlaoder)
    {
        lock (readyList)
        {
            readyList.Enqueue(downlaoder.fileInfo);
        }
    }
    private void DownloadedCallBack(FileDownloader downloader)
    {
        try
        {
            //待更新
            var MD5 = CheckMD5(downloader.fileInfo);
            if (!MD5)
            {
                DeleteFile(downloader.FilePath);
                lock (readyList)
                {
                    readyList.Enqueue(downloader.fileInfo);
                }
            }
            else
            {
                var file = downloader.FilePath;
                if (Path.GetExtension(file) == ".zip")
                {
                    using (ZipFile zip = new ZipFile(file))
                    {
                        var count = zip.Count;
                        foreach (var i in zip)
                        {
                            i.Extract(Application.persistentDataPath + "/update/", ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    DeleteFile(downloader.FilePath);
                }
            }
            lock (readyList)
            {
                if (readyList.Count > 0)
                {
                    //如果是正确的就取出一个继续下载
                    downloader.StartDownload(PathUtil.DownloadPath, readyList.Dequeue());
                }
                else
                {
                    int i = downloaders.IndexOf(downloader);
                    lock (downloadstates)
                    {
                        downloadstates[i] = true;
                        bool Done = true;
                        for (int j = 0; j < downloadThreads; j++)
                        {
                            Done &= downloadstates[j];
                        }
                        if (Done)
                        {
                            actionAllDownloadDone?.Invoke();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    public void Update()
    {
        FileDownloader.Onupdate();
    }
    /// <summary>
    /// 开始下载资源包
    /// </summary>
    private void StartDownloadResPack()
    {
        for (int i = 0; i < downloadThreads; i++)
        {
            lock (readyList)
            {
                if (readyList.Count > 0)
                {
                    var packinfo = readyList.Dequeue();
                    downloaders[i].StartDownload(PathUtil.DownloadPath, packinfo);
                    downloadstates[i] = false;
                }
            }
        }
    }
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
    private bool CheckMD5(PackInfo file)
    {
        var filepath = Path.Combine(PathUtil.DownloadPath, file.fileName);
        byte[] data;
        using (FileStream fs = new FileStream(filepath, FileMode.Open))
        {
            int len = (int)fs.Length;
            data = new byte[len];
            fs.Read(data, 0, len);
        }
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] fileNewMD5 = md5.ComputeHash(data);
        var fileOldMD5 = Encoding.UTF8.GetBytes(file.MD5);
        bool isEqual = false;
        if (fileNewMD5.Length == fileOldMD5.Length)
        {
            int i = 0;
            while ((i < fileOldMD5.Length) && (fileOldMD5[i] == fileNewMD5[i]))
            {
                i += 1;
            }
            if (i == fileOldMD5.Length)
            {
                isEqual = true;
            }
        }

        return isEqual;
    }
    /// <summary>
    /// 请求更新列表，就是下载版本列表
    /// </summary>
    /// <returns></returns>
    private async Task<List<UpdateInfo>> ReqUpdateInfo()
    {
        try
        {
            UnityWebRequest uwr = UnityWebRequest.Get(fullAppUpdateUrl + "update_list.json");
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
        catch (Exception ex)
        {
            throw ex;
        }
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
                needUpdate = true;
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
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;
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
    private bool CheckNetWorkActive()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            return true;
        return false;
    }
    #endregion
}
/// <summary>
/// 版本号信息
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// 版本的版本号
    /// </summary>
    public string appVersion;
    /// <summary>
    /// 版本所包含的资源包
    /// </summary>
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
    /// 包对应的下载地址
    /// </summary>
    public string url;
    /// <summary>
    /// 文件名
    /// </summary>
    public string fileName;
}
using Ionic.Zip;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
public class HotUpdater
{
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
    /// 更新提示语
    /// </summary>
    public Action<string> actionUpdateTipsText;
    /// <summary>
    /// 进度条函数更新
    /// </summary>
    public Action<float> actionDownloadValue;
    /// <summary>
    /// 需要整包更新，也就是重新全部下载
    /// </summary>
    bool needFullAppUpdate;
    /// <summary>
    /// 是否强制更新
    /// </summary>
    bool hasNextUpdateBtn;
    /// <summary>
    /// 整包更新的URL
    /// </summary>
    string fullAppUpdateUrl;
    /// <summary>
    /// 相同版本号的资源更新
    /// </summary>
    bool sameAppVerResUpdate;
    private List<PackInfo> packList = new List<PackInfo>();
    private Downloader downloader;

    private IEnumerator OnDownloadItr;
    /// <summary>
    /// 当前下载包的序号
    /// </summary>
    private int currPackIndex = 0;

    public void Init()
    {
        // 解决HTTPS证书问题
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
    }

    public async void Start()
    {
        needFullAppUpdate = false;
        hasNextUpdateBtn = false;
        fullAppUpdateUrl = "http://1.116.50.215";
        sameAppVerResUpdate = false;

        packList.Clear();
        downloadingInfo = new DownloadingInfo();
        //获得版本列表json
        var updateInfos = await ReqUpdateInfo();
        if (updateInfos != null)
        {
            SortUpdateInfo(ref updateInfos);
            //为后面的更新类型做铺垫
            CalculateUpdatePackList(updateInfos);
            if (needFullAppUpdate)
            {
                Debug.Log("-----------------------需要更新版本-----------------------");
                ///
                ///
                ///
                ///
            }
            else
            {
                if (sameAppVerResUpdate)
                {
                    Debug.Log("开始下载资源包，共:" + packList.Count);
                    //当前下载的包的序号
                    currPackIndex = 0;
                    StartDownloadResPack();
                }
                else
                {
                    Debug.Log("没有任何更新");
                    actionNothongUpdate?.Invoke();
                }
            }
        }
    }

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
        var packInfo = packList[currPackIndex];
        Debug.Log($"下载完毕共下载了{currPackIndex + 1}个包");
        yield return null;
        var file = Application.persistentDataPath + "/" + packInfo.MD5;
        //判断MD5是否正确
        var MD5 = CheckMD5();
        if (MD5)
        {
            int index = 0;
            using (ZipFile zip = new ZipFile(file))
            {
                var count = zip.Count;
                foreach (var i in zip)
                {
                    index++;
                    i.Extract(Application.persistentDataPath + "/update/", ExtractExistingFileAction.OverwriteSilently);
                    yield return null;
                }
            }
            DeleteFile(file);
            VersionManager.Instance.UpdateResVersion(packInfo.resVersion);
            currPackIndex++;
            actionDownloadValue?.Invoke(1);
            StartDownloadResPack();
        }
        else
        {
            DeleteFile(file);
            downloader = null;
            downloader = new Downloader();
            downloader.Start(packList[currPackIndex]);

        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="file"></param>
    private void DeleteFile(string file)
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
    private bool CheckMD5()
    {
        return true;
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
        //if(next)
        //    currPackIndex++;
        if (currPackIndex > packList.Count - 1)
        {
            Debug.Log("所有包下载完毕");
            actionAllDownloadDone?.Invoke();
            downloader.Dispose();
            downloader = null;
            return;
        }
        packList.Sort((a, b) =>
        {
            return VersionManager.Instance.CompareVersion(a.resVersion, b.resVersion);
        });
        var packinfo = packList[currPackIndex];
        downloadingInfo.targetDownloadSize = packinfo.size;
        downloadingInfo.currPackIndex = currPackIndex;
        downloadingInfo.totalPackCount = packList.Count;
        downloader = new Downloader();
        downloader.Start(packinfo);

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
    /// 分析更新的类型
    /// </summary>
    /// <param name="updateinfos"></param>
    private void CalculateUpdatePackList(List<UpdateInfo> updateinfos)
    {
        //Debug.Log(VersionManager.Instance.resVersion);
        //Debug.Log(updateinfos[0].updateList[0].resVersion);
        string bigestVer = VersionManager.Instance.appVersion;
        foreach (UpdateInfo info in updateinfos)
        {
            if (VersionManager.Instance.CompareVersion(info.appVersion, bigestVer) > 0)
            {
                needFullAppUpdate = true;
                bigestVer = info.appVersion;
                fullAppUpdateUrl = info.appUrl;
                return;
            }

            if (VersionManager.Instance.CompareVersion(info.appVersion, bigestVer) == 0)
            {
                hasNextUpdateBtn = true;
                foreach (var pack in info.updateList)
                {
                    if (VersionManager.Instance.CompareVersion(pack.resVersion, VersionManager.Instance.resVersion) > 0)
                    {
                        //将高于资源版本号的添加到下载列表
                        sameAppVerResUpdate = true;
                        packList.Add(pack);
                    }
                }
            }
        }

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
}

class DownloadingInfo
{
    public long currDownloadsize;
    public long targetDownloadSize;
    public int currPackIndex;
    public int totalPackCount;
}
public class UpdateInfo
{
    public string appVersion;
    public string appUrl;
    public List<PackInfo> updateList = new List<PackInfo>();
}
public class PackInfo
{
    /// <summary>
    /// 包对应的MD5
    /// </summary>
    public string MD5;
    /// <summary>
    /// 资源版本号
    /// </summary>
    public string resVersion;
    /// <summary>
    /// 包的大小
    /// </summary>
    public int size;
    /// <summary>
    /// 包对应的下载地址
    /// </summary>
    public string url;
}
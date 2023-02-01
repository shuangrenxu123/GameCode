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
    /// û���κθ���
    /// </summary>
    public Action actionNothongUpdate;
    /// <summary>
    /// ���а���������Ժ�
    /// </summary>
    public Action actionAllDownloadDone;
    /// <summary>
    /// ������ʾ��
    /// </summary>
    public Action<string> actionUpdateTipsText;
    /// <summary>
    /// ��������������
    /// </summary>
    public Action<float> actionDownloadValue;
    /// <summary>
    /// ��Ҫ�������£�Ҳ��������ȫ������
    /// </summary>
    bool needFullAppUpdate;
    /// <summary>
    /// �Ƿ�ǿ�Ƹ���
    /// </summary>
    bool hasNextUpdateBtn;
    /// <summary>
    /// �������µ�URL
    /// </summary>
    string fullAppUpdateUrl;
    /// <summary>
    /// ��ͬ�汾�ŵ���Դ����
    /// </summary>
    bool sameAppVerResUpdate;
    private List<PackInfo> packList = new List<PackInfo>();
    private Downloader downloader;

    private IEnumerator OnDownloadItr;
    /// <summary>
    /// ��ǰ���ذ������
    /// </summary>
    private int currPackIndex = 0;

    public void Init()
    {
        // ���HTTPS֤������
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
        //��ð汾�б�json
        var updateInfos = await ReqUpdateInfo();
        if (updateInfos != null)
        {
            SortUpdateInfo(ref updateInfos);
            //Ϊ����ĸ����������̵�
            CalculateUpdatePackList(updateInfos);
            if (needFullAppUpdate)
            {
                Debug.Log("-----------------------��Ҫ���°汾-----------------------");
                ///
                ///
                ///
                ///
            }
            else
            {
                if (sameAppVerResUpdate)
                {
                    Debug.Log("��ʼ������Դ������:" + packList.Count);
                    //��ǰ���صİ������
                    currPackIndex = 0;
                    StartDownloadResPack();
                }
                else
                {
                    Debug.Log("û���κθ���");
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
                    Debug.LogError("���ش���");
                    break;
                case Downloader.DownloadState.DataProcessingError:
                    Debug.LogError("���ش���");
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
        //������һ�����أ�����ѹ�Ѿ����غ��˵�ѹ������Ȼ��ʼ�ж���һ������
        RunCorotine();

    }

    private void RunCorotine()
    {
        //movenext������Я��
        if (OnDownloadItr != null && !OnDownloadItr.MoveNext())
        {
            OnDownloadItr = null;
        }
    }
    /// <summary>
    /// һ������������Ժ�
    /// </summary>
    private IEnumerator OnDownloadEnd()
    {
        var packInfo = packList[currPackIndex];
        Debug.Log($"������Ϲ�������{currPackIndex + 1}����");
        yield return null;
        var file = Application.persistentDataPath + "/" + packInfo.MD5;
        //�ж�MD5�Ƿ���ȷ
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
    /// ɾ���ļ�
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
    ///�����е�ʱ�������½�����
    ///</summary>
    public void OnDownloading()
    {
        //��õ�ǰ�����˵��ļ�
        downloadingInfo.currDownloadsize = downloader.currDownloadSize;
        float value = (float)downloadingInfo.currDownloadsize / downloadingInfo.targetDownloadSize;
        actionDownloadValue?.Invoke(value);

    }
    /// <summary>
    /// ��ʼ������Դ��
    /// </summary>
    public void StartDownloadResPack()
    {
        //if(next)
        //    currPackIndex++;
        if (currPackIndex > packList.Count - 1)
        {
            Debug.Log("���а��������");
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
    /// ���б��еİ�����������
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
    /// ��������б��������ذ汾�б�
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
        //���̵߳ȴ����ؽ��
        if (uwr.error != null)
        {
            Debug.LogError("û���յ������б�");
            actionNothongUpdate.Invoke();
            return null;
        }
        return JsonMapper.ToObject<List<UpdateInfo>>(uwr.downloadHandler.text);
    }

    /// <summary>
    /// �������µ�����
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
                        //��������Դ�汾�ŵ���ӵ������б�
                        sameAppVerResUpdate = true;
                        packList.Add(pack);
                    }
                }
            }
        }

    }

    /// <summary>
    /// ���HTTPs����
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
    /// ����Ӧ��MD5
    /// </summary>
    public string MD5;
    /// <summary>
    /// ��Դ�汾��
    /// </summary>
    public string resVersion;
    /// <summary>
    /// ���Ĵ�С
    /// </summary>
    public int size;
    /// <summary>
    /// ����Ӧ�����ص�ַ
    /// </summary>
    public string url;
}
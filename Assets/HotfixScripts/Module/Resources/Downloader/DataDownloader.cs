using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class DataDownloader
{
    private static readonly List<Tuple<Action<DataDownloader>, DataDownloader>> OnCompleteQueue = new List<Tuple<Action<DataDownloader>, DataDownloader>>();
    public DownloadStatus status { get; private set; } = DownloadStatus.Downloading;

    public byte[] Data
    {
        get
        {
            if (status != DownloadStatus.Succeeded)
                Debug.LogError("DataDownloader.Data : ������ֻ���� Status ��ֵΪ DownloadStatus.Succeeded ʱ�ɷ���");
            return PrivateData;
        }
    }
    private byte[] PrivateData;
    /// <summary> ��ǰ�����ص��ֽ��� </summary>
    public int DownloadedBytes { get; private set; }
    /// <summary> ��Ҫ���ص����ֽ��� ( ����ʹ���û����õ�ֵ, ���û�û������������� Http Headers ����, �����ʧ����Ϊ 0 ) </summary>
    public int DownloadTotalBytes
    {
        get => DownloadTotalBytesByUserSet > 0 ? DownloadTotalBytesByUserSet : DownloadTotalBytesByHeaders;
        set
        {
            if (value <= 0)
            {
                Debug.LogError("DataDownloader.DownloadTotalBytes : �����Եĸ�ֵ������� 0");
            }
            else
            {
                DownloadTotalBytesByUserSet = value;
            }
        }
    }
    private int DownloadTotalBytesByUserSet = 0;
    private int DownloadTotalBytesByHeaders = 0;

    /// <summary>
    /// �ɹ����صĻص�����
    /// </summary>
    private readonly Action<DataDownloader> OnSucceeded;
    /// <summary>
    /// ����ʧ�ܵĻص�����
    /// </summary>
    private readonly Action<DataDownloader> OnFailed;
    /// <summary>
    /// �Ƿ����߳���ִ�лص�
    /// </summary>
    private readonly bool MainThreadInvoke;

    /// <summary>
    /// �Ƿ���ȡ������
    /// </summary>
    private bool IsCanceled;

    /// <summary> ȡ������ </summary>
    public void Cancel() => IsCanceled = true;

    public static void OnUpdate()
    {
        List<Tuple<Action<DataDownloader>, DataDownloader>> onCompletedQueue = null;
        lock (OnCompleteQueue)
        {
            if (OnCompleteQueue.Count > 0)
            {
                onCompletedQueue = OnCompleteQueue.GetRange(0, OnCompleteQueue.Count);
                OnCompleteQueue.Clear();
            }

            if (onCompletedQueue != null)
            {
                int length = onCompletedQueue.Count;
                for (int i = 0; i < length; i++)
                {
                    onCompletedQueue[i].Item1(onCompletedQueue[i].Item2);
                }
            }
        }
    }

    public DataDownloader(Action<DataDownloader> OnSucceeded, Action<DataDownloader> OnFailed, bool mainThreadInvoke)
    {
        this.OnSucceeded = OnSucceeded;
        this.OnFailed = OnFailed;
        this.MainThreadInvoke = mainThreadInvoke;
    }

    public async void StartDownload(string url)
    {
        HttpClient httpClient = HttpClientPool.GetHttpClient();
        try
        {
            using (HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                    throw new Exception("HttpStatusCode : " + (int)response.StatusCode + " " + response.StatusCode);
                DownloadTotalBytesByHeaders = response.Content.Headers.ContentLength == null ? 0 : (int)response.Content.Headers.ContentLength;
                using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                PrivateData = await StreamCopy(responseStream).ConfigureAwait(false);
            }
            DownloadCompleted(IsCanceled ? DownloadStatus.Cancelled : DownloadStatus.Succeeded);
        }
        catch (Exception E)
        {
            DownloadCompleted(DownloadStatus.Failed);
        }
        finally
        {
            httpClient.CancelPendingRequests();
            HttpClientPool.RecoverHttpClient(httpClient);
        }
    }

    private void DownloadCompleted(DownloadStatus s)
    {
        this.status = s;
        Action<DataDownloader> action = null;
        if (status == DownloadStatus.Succeeded)
        {
            action = OnSucceeded;
        }
        else if (status == DownloadStatus.Failed)
        {
            action = OnFailed;
        }
        if (action != null)
        {
            if (MainThreadInvoke)
            {
                lock (OnCompleteQueue)
                {
                    try
                    {
                        OnCompleteQueue.Add(new Tuple<Action<DataDownloader>, DataDownloader>(action, this));
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
            else
            {
                action.Invoke(this);
            }
        }
    }

    private async Task<byte[]> StreamCopy(Stream responseStream)
    {
        byte[] buffer = new byte[16384];
        using MemoryStream memoryStream = new MemoryStream();
        while (true)
        {
            int bytesRead = await responseStream.ReadAsync(buffer).ConfigureAwait(false);
            if (bytesRead == 0 || IsCanceled) break;
            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
            DownloadedBytes += bytesRead;
        }
        return IsCanceled ? null : memoryStream.ToArray();
    }
}

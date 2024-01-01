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
                Debug.LogError("DataDownloader.Data : 此属性只有在 Status 的值为 DownloadStatus.Succeeded 时可访问");
            return PrivateData;
        }
    }
    private byte[] PrivateData;
    /// <summary> 当前已下载的字节数 </summary>
    public int DownloadedBytes { get; private set; }
    /// <summary> 需要下载的总字节数 ( 优先使用用户设置的值, 如用户没有主动设置则从 Http Headers 查找, 如查找失败则为 0 ) </summary>
    public int DownloadTotalBytes
    {
        get => DownloadTotalBytesByUserSet > 0 ? DownloadTotalBytesByUserSet : DownloadTotalBytesByHeaders;
        set
        {
            if (value <= 0)
            {
                Debug.LogError("DataDownloader.DownloadTotalBytes : 此属性的赋值必须大于 0");
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
    /// 成功下载的回调函数
    /// </summary>
    private readonly Action<DataDownloader> OnSucceeded;
    /// <summary>
    /// 下载失败的回调函数
    /// </summary>
    private readonly Action<DataDownloader> OnFailed;
    /// <summary>
    /// 是否主线程上执行回调
    /// </summary>
    private readonly bool MainThreadInvoke;

    /// <summary>
    /// 是否已取消下载
    /// </summary>
    private bool IsCanceled;

    /// <summary> 取消下载 </summary>
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

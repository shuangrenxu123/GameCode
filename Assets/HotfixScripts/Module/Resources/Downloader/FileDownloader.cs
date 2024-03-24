using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Downloader
{
    public class FileDownloader
    {
        /// <summary>
        /// 完成执行函数队列。多个httpclient完成后的回调，在主线程统一执行
        /// </summary>
        private static readonly List<Tuple<Action<FileDownloader>, FileDownloader>> OnCompleteQueue = new List<Tuple<Action<FileDownloader>, FileDownloader>>();
        public DownloadStatus status { get; private set; } = DownloadStatus.Downloading;
        /// <summary>
        /// 成功下载的回调函数
        /// </summary>
        private readonly Action<FileDownloader> OnSucceeded;
        /// <summary>
        /// 下载失败的回调函数
        /// </summary>
        private readonly Action<FileDownloader> OnFailed;
        /// <summary>
        /// 是否主线程上执行回调
        /// </summary>
        private readonly bool MainThreadInvoke;
        public string FilePath { get; private set; }
        /// <summary>
        /// 当前下载了的字节数量
        /// </summary>
        public int DownloadBytes { get; private set; }
        /// <summary>
        /// 总共需要下载的字节数量，可以自行设置想要的数量，如果没有设置则默认为文件大小
        /// </summary>
        public int DownloadTotalBytes
        {
            get
            {
                return DownloadTotalBytesByUserSet > 0 ? DownloadTotalBytesByUserSet : DownloadTotalBytesByHeaders;
            }
            set
            {
                if (value <= 0)
                {
                    throw new Exception("下载的数值不能 <= 0");
                }
                else
                {
                    DownloadTotalBytesByUserSet = value;
                }
            }
        }
        /// <summary>
        /// 用户设置的值，但依然是通过DownloadTotalBytes来设置
        /// </summary>
        private int DownloadTotalBytesByUserSet = 0;
        /// <summary>
        /// 从http headr获得文件大小
        /// </summary>
        private int DownloadTotalBytesByHeaders = 0;
        public PackInfo fileInfo = null;
        /// <summary>
        /// 是否取消了下载
        /// </summary>
        private bool isCanceled;
        /// <summary>
        /// 取消下载
        /// </summary>
        public void Cancle()
        {
            isCanceled = true;
        }
        public FileDownloader(Action<FileDownloader> OnSucceeded, Action<FileDownloader> OnFailed, bool mainThreadInvoke)
        {
            this.OnSucceeded = OnSucceeded;
            this.OnFailed = OnFailed;
            this.MainThreadInvoke = mainThreadInvoke;
        }
        /// <summary>
        /// 主线程上来执行回调函数
        /// </summary>
        public static void Onupdate()
        {
            List<Tuple<Action<FileDownloader>, FileDownloader>> onCompletedQueue = null;
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
        /// <summary>
        /// 异步下载文件。
        /// </summary>
        /// <param name="URL">文件地址（具体到文件名，包括后缀）</param>
        /// <param name="directory">要保存的文件夹</param>
        public async void StartDownload(string directory, PackInfo info)
        {
            HttpClient client = HttpClientPool.GetHttpClient();
            try
            {
                fileInfo = info;
                Directory.CreateDirectory(directory);
                FilePath = directory + fileInfo.fileName;
                //先异步获得http 的文件头
                using HttpResponseMessage response = await client.GetAsync(fileInfo.url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("HttpStatusCode : " + (int)response.StatusCode + " " + response.StatusCode);
                }
                //获得文件大小
                DownloadTotalBytesByHeaders = response.Content.Headers.ContentLength == null ? 0 : (int)response.Content.Headers.ContentLength;
                //获得相关的文件流
                using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                await StreamCopy(responseStream);

                DownloadCompleted(isCanceled ? DownloadStatus.Cancelled : DownloadStatus.Succeeded);
            }
            catch (Exception e)
            {
                DownloadCompleted(DownloadStatus.Failed);
                throw e;
            }
            finally
            {
                //取消http请求以达到复用的目的
                client.CancelPendingRequests();
                //回收httpclient
                HttpClientPool.RecoverHttpClient(client);
            }
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        private async Task StreamCopy(Stream responseStream)
        {
            //Tcp接收缓冲区大小
            byte[] buffer = new byte[16384];
            using FileStream fileStream = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 16384, true);
            while (true)
            {
                int byteRead = await responseStream.ReadAsync(buffer).ConfigureAwait(false);
                if (byteRead == 0 || isCanceled)
                {
                    break;
                }
                await fileStream.WriteAsync(buffer.AsMemory(0, byteRead)).ConfigureAwait(false);
                DownloadBytes += byteRead;
            }
        }
        /// <summary>
        /// 下载完成后的回调
        /// </summary>
        /// <param name="s"></param>
        private void DownloadCompleted(DownloadStatus s)
        {
            this.status = s;
            Action<FileDownloader> action = null;
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
                            OnCompleteQueue.Add(new Tuple<Action<FileDownloader>, FileDownloader>(action, this));
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
    }

    public enum DownloadStatus
    {
        /// <summary> 正在下载 </summary>
        Downloading = 0,

        /// <summary> 取消下载 </summary>
        Cancelled = 1,

        /// <summary> 下载失败 </summary>
        Failed = 2,

        /// <summary> 下载成功 </summary>
        Succeeded = 3
    }
}
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
public class Downloader
{
    private Stream fs, ns;
    private int readSize;
    private byte[] readBuff;
    public PackInfo packInfo;
    private Thread thread;//写文件的线程
    private bool stopThread = false;

    public long currDownloadSize;

    public DownloadState state;
    public enum DownloadState
    {
        Ready,
        ConnectionError,//连接错误
        DataProcessingError,//下载错误
        Ing,
        End,
    }
    public void Start(PackInfo info)
    {
        readBuff = new byte[1024 * 4];
        state = DownloadState.Ready;
        packInfo = info;
        var httpReq = WebRequest.Create(info.url) as HttpWebRequest;
        httpReq.Timeout = 5000;

        //临时的数据保存
        var savePath = Application.persistentDataPath + "/" + info.MD5;

        fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
        //当前文件大小
        currDownloadSize = fs.Length;
        //相同也即下载完成
        if (currDownloadSize == info.size)
        {
            state = DownloadState.End;
            Debug.Log("下载完成");
            Reset();
            return;
        }
        //如果文件里面的数据比包的大小大则说明出错了
        else if (currDownloadSize > info.size)
        {
            currDownloadSize = 0;
            fs.Close();
            File.Delete(savePath);
            fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        }
        //说明已经下载过
        else if (currDownloadSize > 0)
        {
            //设置从哪开始写
            fs.Seek(currDownloadSize, SeekOrigin.Begin);
            httpReq.AddRange(currDownloadSize);
        }
        
        HttpWebResponse response;
        try
        {
            //获得回应。即获得下载的字节
            response = (HttpWebResponse)httpReq.GetResponse();
        }
        catch (System.Exception e)
        {
            state = DownloadState.ConnectionError;
            Debug.LogError(e);
            Reset();
            return;
        }
        //如果不成功则重新下载
        if (response.StatusCode != HttpStatusCode.PartialContent)
        {
            if (File.Exists(savePath))
            {
                fs.Close();
                fs = null;
                currDownloadSize = 0;
                File.Delete(savePath);
            }
            fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        }
        //获得文件流
        ns = response.GetResponseStream();
        if (thread == null)
        {
            stopThread = false;
            thread = new Thread(WriteThread);
            thread.Start();
        }
    }

    private void WriteThread(object obj)
    {
        state = DownloadState.Ing;
        while (!stopThread)
        {
            try
            {
                var readSize = ns.Read(readBuff, 0, readBuff.Length);
                if (readSize > 0)
                {
                    fs.Write(readBuff, 0, readSize);
                    currDownloadSize += readSize;
                    Thread.Sleep(0);
                }
                else
                {
                    stopThread = true;
                    fs.Dispose();
                    state = DownloadState.End;
                    Reset();
                }
            }
            catch (System.Exception e)
            {

                state = DownloadState.DataProcessingError;
                Debug.LogError(e);
                Reset();
            }
        }
    }
    //重置
    public void Reset()
    {
        if (fs != null)
        {
            fs.Dispose();
            fs = null;
        }
        if (ns != null)
        {
            ns.Dispose();
            ns = null;
        }
        stopThread = true;
        //thread = null;
        readBuff = null;
        packInfo = null;
    }
}

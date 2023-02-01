using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
public class Downloader
{
    private Stream fs, ns;
    private int readSize;
    private byte[] readBuff;
    private PackInfo packInfo;
    private Thread thread;//д�ļ����߳�
    private bool stopThread = false;

    public long currDownloadSize;

    public DownloadState state;
    public enum DownloadState
    {
        Ready,
        ConnectionError,//���Ӵ���
        DataProcessingError,//���ش���
        Ing,
        End,
    }
    public void Start(PackInfo info)
    {
        readBuff = new byte[1024 * 4];
        state = DownloadState.Ready;
        packInfo = info;
        var httpReq = HttpWebRequest.Create(info.url) as HttpWebRequest;
        httpReq.Timeout = 5000;

        //��ʱ�����ݱ���
        var savePath = Application.persistentDataPath + "/" + info.MD5;

        fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
        currDownloadSize = fs.Length;
        //��ͬҲ���������
        if (currDownloadSize == info.size)
        {
            state = DownloadState.End;
            Debug.Log("�������");
            Dispose();
            return;
        }
        //����ļ���������ݱȰ��Ĵ�С����˵��������
        else if (currDownloadSize > info.size)
        {
            currDownloadSize = 0;
            fs.Close();
            File.Delete(savePath);
            fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        }
        //˵���Ѿ����ع�
        else if (currDownloadSize > 0)
        {
            //���ô��Ŀ�ʼд
            fs.Seek(currDownloadSize, SeekOrigin.Begin);
            httpReq.AddRange(currDownloadSize);
        }
        HttpWebResponse response;
        try
        {
            response = (HttpWebResponse)httpReq.GetResponse();
        }
        catch (System.Exception e)
        {
            state = DownloadState.ConnectionError;
            Debug.LogError(e);
            return;
        }
        //������ɹ�����������
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

                }
            }
            catch (System.Exception e)
            {

                state = DownloadState.DataProcessingError;
                Debug.LogError(e);
                Dispose();
            }
        }
    }

    public void Dispose()
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

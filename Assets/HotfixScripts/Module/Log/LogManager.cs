using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class LogManager : ModuleSingleton<LogManager>, IModule
{
    private const string TimeFormat = "hh:mm:ss";
    private const string Timeformat2 = "yyyyMMdd";
    /// <summary>
    /// ���������д���ļ�
    /// </summary>
    private const int logWriteCount = 10;
    /// <summary>
    /// ��������д���ļ�
    /// </summary>
    private const int logWriteTime = 10; 
    private FileStream fileStream;
    private StreamWriter streamWriter;
    private string path = Application.streamingAssetsPath;
    /// <summary>
    /// ���ڵ��������
    /// </summary>
    private int maxDay;
    private StringBuilder stringBuilder;


    private int logCount = 0;

    public void OnCreate(object createParam)
    {
        Application.logMessageReceivedThreaded += LogMessageReceived;
        CreateLogFile();
    }


    public void OnUpdate()
    {
    }
    public void CreateLogFile()
    {
        var filePath = path + "/" + DateTime.Now.ToString(Timeformat2)+".txt";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

#if !UNITY_EDITOR
            //�Ǳ���������½�����־����ɾ���߼�
            DeleteOldLogFiles();
#endif
        fileStream = new FileStream(filePath, FileMode.OpenOrCreate,FileAccess.ReadWrite);
        streamWriter = new StreamWriter(fileStream);
        stringBuilder = new StringBuilder();
    }
    /// <summary>
    /// ɾ��������־�ļ�
    /// </summary>
    private void DeleteOldLogFiles() 
    {
        string[] fils = Directory.GetFiles(path);
        DateTime time = DateTime.Now;

        for (int i = 0; i < fils.Length; i++)
        {

        }
    }
    private void LogMessageReceived(string logstring, string stackTrace, LogType type)
    {
        stringBuilder.Clear();
        //��¼��ʼ�����Ϣ ����+ʱ��+log
        stringBuilder.Append($"[{type}]  [{DateTime.Now.ToString(TimeFormat)}] {logstring}");
        logCount++;
        StackTrace stack = new StackTrace(true);
        bool isDebugLog = false;
        for (int i = 0; i < stack.FrameCount; i++)
        {
            StackFrame sf = stack.GetFrame(i);
            if (isDebugLog) {
                stringBuilder.Append($"   at [{sf.GetMethod().DeclaringType.FullName} : {sf.GetMethod().Name}() in Line:{sf.GetFileLineNumber()}]");
                break;
            }
            else
            {
                string fullName = sf.GetMethod().DeclaringType.FullName;
                if (fullName.EndsWith("UnityEngine.Debug"))
                {
                    isDebugLog = true;
                }
                else if(fullName.EndsWith("UniLog") && sf.GetMethod().Name.Equals("WriteLine"))
                {
                    isDebugLog= true;
                }
            }
        }
        streamWriter.WriteLine(stringBuilder.ToString());

        if (type != LogType.Log || logCount >= logWriteCount)
        {
            logCount = 0;
            WriteFile();
        }

    }
    /// <summary>
    /// ����������д���ļ���
    /// </summary>
    public void WriteFile()
    {
        streamWriter.Flush();
    }
    private async void WriteFileAsync()
    {
        await streamWriter.FlushAsync();
    }
    public void Dispose()
    {
        WriteFile();
        streamWriter.Dispose();
        fileStream.Dispose();
    }
}


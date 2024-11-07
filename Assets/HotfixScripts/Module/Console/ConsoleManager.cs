using Network;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace ConsoleLog
{
    public class ConsoleManager : ModuleSingleton<ConsoleManager>, IModule
    {
        LogFileMoudle logFileManager;

        public event Action<string, string> OnOutput;
        public event Action OnSubmit;
        public void OnCreate(object p)
        {
            logFileManager = new();
#if !UNITY_EDITOR
            Application.logMessageReceivedThreaded += LogMessageReceived;
#endif
            NetWorkManager.Instance.RegisterHandle(5, ReceiveNetMessage);
        }
        /// <summary>
        /// �������յ���������Ϣ
        /// </summary>
        /// <param name="arg0"></param>
        private void ReceiveNetMessage(DefaultNetWorkPackage arg0)
        {
            var message = arg0.Msgobj as PlayerInfo.PlayerMessage;
            if (message != null)
            {
                OutputToConsole(message.Mes);
            }
        }
        private void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            logFileManager.LogMessageReceived(condition, stackTrace, type);
        }
        /// <summary>
        /// �ֶ�д��Log
        /// </summary>
        /// <param name="info"></param>
        public void WriteLogFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Log);
        }
        /// <summary>
        /// �ֶ�д��Error
        /// </summary>
        public void WriteErrorFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Error);
        }
        /// <summary>
        /// �ֶ�д��Warning
        /// </summary>
        /// <param name="info"></param>
        public void WriteWarningFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Warning);
        }
        /// <summary>
        /// �ύ����
        /// </summary>
        /// <param name="command"></param>
        public void SubmitCommand(string command)
        {
            OnSubmit?.Invoke();
        }
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="info"></param>
        /// <param name="color"></param>
        public void OutputToConsole(string info, string color = "#FFFFFF")
        {
            OnOutput?.Invoke(info, color);
        }
        public void OnUpdate()
        {

        }
        public void Dispose()
        {
#if !UNITY_EDITOR
            logFileManager.Dispose();
#endif
        }
    }
}

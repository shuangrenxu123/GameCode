using Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;
namespace ConsoleLog
{
    public class ConsoleManager : ModuleSingleton<ConsoleManager>, IModule
    {
        LogFileMoudle logFileManager;
        CommandModule commonController;

        public List<string> CommandsNames =>commonController.CommandNames;

        public event Action<string,string> OnOutput;
        public event Action OnSubmit;
        public void OnCreate(object p)
        {
            logFileManager = new();
            commonController = new CommandModule();
#if !UNITY_EDITOR
            Application.logMessageReceivedThreaded += LogMessageReceived;
#endif
            NetWorkManager.Instance.RegisterHandle(5, ReceiveNetMessage);
        }
        /// <summary>
        /// 处理接收到的网络消息
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
        /// 手动写入Log
        /// </summary>
        /// <param name="info"></param>
        public void WriteLogFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Log);
        }
        /// <summary>
        /// 手动写入Error
        /// </summary>
        public void WriteErrorFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Error);
        }
        /// <summary>
        /// 手动写入Warning
        /// </summary>
        /// <param name="info"></param>
        public void WriteWarningFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Warning);
        }
        /// <summary>
        /// 提交命令
        /// </summary>
        /// <param name="command"></param>
        public void SubmitCommand(string command)
        {  
            var result = commonController.Execute(command);
            OutputToConsole(result);
            OnSubmit?.Invoke();
        }
        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="info"></param>
        /// <param name="color"></param>
        public void OutputToConsole(string info,string color = "#FFFFFF")
        {
            OnOutput?.Invoke(info,color);
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

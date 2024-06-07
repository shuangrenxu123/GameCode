using Network;
using System;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;
namespace ConsoleLog
{
    public class ConsoleManager : ModuleSingleton<ConsoleManager>, IModule
    {
        LogFileMoudle logFileManager;
        CommandModule commonController;

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
        public void WriteLogFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Log);
        }
        public void WriteErrorFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Error);
        }
        public void WriteWarningFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Warning);
        }
        public void SubmitCommand(string command)
        {  
            var result = commonController.Execute(command);
            OutputToConsole(result);
            OnSubmit?.Invoke();
        }
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

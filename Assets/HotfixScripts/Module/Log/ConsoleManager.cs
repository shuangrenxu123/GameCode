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
        public void OnCreate(object p)
        {
            logFileManager = new();
            commonController = new CommandModule();
#if !UNITY_EDITOR
            Application.logMessageReceivedThreaded += LogMessageReceived;
#endif
        }

        private void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            logFileManager.LogMessageReceived(condition, stackTrace, type);
        }
        public void WriteLogFile(string info)
        {
            logFileManager.LogMessageReceived(info, null, LogType.Log);
        }
        public void SubmitCommand(string command)
        {  
            var result = commonController.Execute(command);
            Output(result);
        }
        private void Output(string info,string color = "#FFFFFF")
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

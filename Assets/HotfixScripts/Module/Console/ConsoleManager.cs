using System;
using System.Collections.Generic;
using Helper;
using UnityEngine;

namespace ConsoleLog
{
    public class ConsoleManager : ModuleSingleton<ConsoleManager>, IModule
    {
        LogFileModule logFileManager;
        ScriptEngine scriptEngine;

        public event Action<string, string> OnOutput;
        public event Action OnSubmitCommand;

        public void OnCreate(object p)
        {
            logFileManager = new LogFileModule();
            scriptEngine = new ScriptEngine();
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
            ExecutionResult result = scriptEngine.Execute(command);
            if (!result.Success)
            {
                OutputToConsole(result.Error, ColorUtility.ToHtmlStringRGB(new Color(0.96f, 0.39f, 0.39f)));
            }

            OnSubmitCommand?.Invoke();
        }

        public void OutputToConsole(string info, string color = "FFFFFF")
        {
            OnOutput?.Invoke(info, color);
        }

        public List<string> MatchCommands(string keyword)
        {
            if (scriptEngine == null)
            {
                return new List<string>();
            }

            return scriptEngine.MatchCommands(keyword) ?? new List<string>();
        }

        public List<CommandSuggestion> MatchCommandSuggestions(string keyword)
        {
            if (scriptEngine == null)
            {
                return new List<CommandSuggestion>();
            }

            return scriptEngine.MatchCommandSuggestions(keyword) ?? new List<CommandSuggestion>();
        }

        public void RegisterCommand(string name, string displayText, Func<List<object>, object> handler)
        {
            if (scriptEngine == null)
            {
                throw new InvalidOperationException("控制台脚本引擎尚未初始化");
            }

            scriptEngine.RegisterCommand(name, displayText, handler);
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

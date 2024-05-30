using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace ConsoleLog
{
    public class Command
    {
        public struct Context
        {
            public bool success;
            public string message;
            public void SetValua(bool success,string mes)
            {
                this.success = success;
                this.message = mes;
            }
        }
        public string name;
        public MethodInfo method;
        public ParameterInfo[] parameters;
        public Command(string name, MethodInfo method)
        {
            this.name = name;
            this.method = method;
            parameters = method.GetParameters();
        }
        public Context Execute(string[] args)
        {
            var context = new Context();
            object[] loadedParams = new object[parameters.Length];
            for (int i = 0; i < loadedParams.Length; i++)
            {
                if (i >= args.Length)
                {
                    if (parameters[i].HasDefaultValue)
                    {
                        loadedParams[i] = parameters[i].DefaultValue;
                        continue;
                    }
                    else
                    {
                        context.SetValua(false,"命令参数不正确");
                        return context;
                    }
                }
                if (CommandParameterHandle.ParseParameter(args[i], parameters[i].ParameterType, out loadedParams[i]))
                {
                    continue;
                }
                else
                {
                    context.SetValua(false, "没有找到参数转化类型");
                    return context;
                }
            }
            method.Invoke(null, loadedParams);
            context.SetValua(true, null);
            return context;
        }

    }
    public class CommandModule
    {
        private Dictionary<string, Command> commands;
        public CommandModule()
        {
            commands = new Dictionary<string, Command>();
            var lists = LoadCommands();
            Debug.Log(lists.Count);
            foreach (var c in lists)
            {
                commands.Add(c.name, c);
            }
        }
        public string Execute(string c)
        {
            var common = ProcessInput(c);
            if (!commands.TryGetValue(common[0], out Command command))
            {
                return "没有找到命令";
            }
            var args = new Span<string>(common, 1, common.Length - 1);
            var context = command. Execute(args.ToArray());
            return context.message;
        }
        private List<Command> LoadCommands()
        {
            Type[] totalType = Assembly.GetExecutingAssembly().GetTypes();
            List<Command> commands = new List<Command>();
            foreach (var type in totalType)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (methods.Length == 0)
                {
                    continue;
                }
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<CommandAttribute>();
                    if (attribute == null)
                        continue;
                    var command = CreateCommand(method, attribute);
                    if (command != null)
                        commands.Add(command);
                }
            }
            return commands;

        }
        private Command CreateCommand(MethodInfo methodInfo, CommandAttribute attr)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    foreach (var par in parameters)
                    {
                        if (!CommandParameterHandle.ContainsParser(par.ParameterType))
                            return null;
                    }
                }
            }
            string commandName = attr.Name;
            Command command = new Command(attr.Name, methodInfo);
            return command;
        }

        private string[]  ProcessInput(string input)
        {
            return input.Split(' ');
        }
        private void QueryCommands()
        {

        }
    }
}
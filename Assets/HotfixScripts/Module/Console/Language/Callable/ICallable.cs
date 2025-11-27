using System.Collections.Generic;
using System.Reflection;
using ConsoleLog;
using UnityEngine;


namespace Helper
{
    public interface ICallable
    {
        public abstract void Execute(List<string> args);
    }


    public class MethodCallable : ICallable
    {
        public string name;
        public MethodInfo method;
        public ParameterInfo[] parameters;
        public string description;
        public MethodCallable(string name, MethodInfo method, string description = null)
        {
            this.name = name;
            this.method = method;
            this.description = description;
            parameters = method.GetParameters();
        }
        public void Execute(List<string> args)
        {
            object[] loadedParams = new object[parameters.Length];
            for (int i = 0; i < loadedParams.Length; i++)
            {
                if (i >= args.Count)
                {
                    if (parameters[i].HasDefaultValue)
                    {
                        loadedParams[i] = parameters[i].DefaultValue;
                        continue;
                    }

                }
                if (CommandParameterHandle.ParseParameter(args[i], parameters[i].ParameterType, out loadedParams[i]))
                {
                    continue;
                }
                else
                {
                    throw new System.Exception("参数解析错误");
                }
            }
            method.Invoke(null, loadedParams);
        }
    }
}



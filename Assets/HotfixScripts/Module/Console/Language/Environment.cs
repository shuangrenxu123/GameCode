using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleLog;
using UnityEngine;

namespace Helper
{
    public class Environment
    {

        /// <summary>
        /// 外部环境
        /// </summary>
        Environment enclosing;


        public Environment()
        {
            enclosing = null;
            LoadMethod();
        }
        public Environment(Environment environment)
        {
            enclosing = environment;
        }
        private Dictionary<int, object> variables = new();
        private Dictionary<int, ICallable> callable = new();
        public object GetVariables(Token token)
        {
            var hashCode = token.sourceString.GetHashCode();
            //现在自己的环境中寻找
            if (variables.TryGetValue(hashCode, out object value))
            {
                return value;
            }
            // 如果有外层环境,那就尝试去外层环境找找
            if (enclosing != null)
                return enclosing.GetVariables(token);
            throw new RuntimeException(token, $"变量未定义{token.sourceString}");
        }
        public MethodCallable GetMethod(string methodName)
        {
            var hashCode = methodName.GetHashCode();

            if (callable.TryGetValue(hashCode, out var value))
            {
                return value as MethodCallable;
            }
            throw new Exception($"未找到该方法{methodName}");
        }

        /// <summary>
        /// 定义一个变量
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="variable"></param>
        public void Define(string varName, object variable)
        {
            int hashCode = varName.GetHashCode();
            variables[hashCode] = variable;
        }

        public void Assign(Token name, object variable)
        {
            int hashCode = name.sourceString.GetHashCode();
            //如果是本环境的值,那就更新一下
            if (variables.ContainsKey(hashCode))
            {
                variables[hashCode] = variable;
                return;
            }
            //尝试更新外层环境
            if (enclosing != null)
            {
                enclosing.Assign(name, variable);
                return;
            }
            throw new RuntimeException(name, $"变量未定义{name.sourceString}");
        }


        void LoadMethod()
        {
            Type[] totalType = Assembly.GetExecutingAssembly().GetTypes();
            // List<MethodCallable> commands = new List<MethodCallable>();
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
                    var command = LoadMethod(method, attribute);
                    if (command != null)
                    {
                        var hashCode = command.name.GetHashCode();
                        callable.Add(hashCode, command);
                    }
                }
            }
            //return commands;
        }
        private MethodCallable LoadMethod(MethodInfo methodInfo, CommandAttribute attr)
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
            MethodCallable command = new(attr.Name, methodInfo);
            return command;
        }
    }
}
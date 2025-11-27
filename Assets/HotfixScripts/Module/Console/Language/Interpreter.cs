using System;
using System.Collections.Generic;
using ConsoleLog;
using UnityEngine;

namespace Helper
{
    public class Interpreter
    {
        ExpressionInterpreter exprInterpreter = new();
        Scanner scanner = new();

        ExpressionParser expressionParser = new();

        public void ExecuteCommand(string sourceString)
        {
            try
            {
                var tokens = scanner.Parser(sourceString);
                expressionParser.Init(tokens);
                var expr = expressionParser.Expression();

                exprInterpreter.Interpret(expr);
            }
            catch (Exception e)
            {
                ConsoleManager.Instance.OutputToConsole(e.Message);
            }
        }

        public void RegisterVariable(string name, object instance, bool readOnly = false)
        {
            exprInterpreter.RegisterVariable(name, instance, readOnly);
        }

        public void RegisterVariable(string name, Func<object> getter, Action<object> setter = null, Type declaredType = null)
        {
            exprInterpreter.RegisterVariable(name, getter, setter, declaredType);
        }

        public List<string> MatchCommands(string keyword)
        {
            return exprInterpreter.MatchCommands(keyword);
        }


    }
}

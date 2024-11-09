using System;
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


    }
}

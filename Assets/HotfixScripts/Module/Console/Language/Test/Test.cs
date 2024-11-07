using UnityEngine;
using Helper;


public class Test : MonoBehaviour
{
    ExpressionInterpreter ExpressionInterpreter = new ExpressionInterpreter();
    void Start()
    {
        var Scanner = new Scanner();
        var tokens = Scanner.Parser("giveItem(1+2,2)");

        var parser = new ExpressionParser(tokens);
        var a = parser.Expression();
        ExpressionInterpreter.Interpret(a);
    }
}

using UnityEngine;

namespace Helper
{
    public enum TokenType
    {
        //=============单字符Token===================
        Left_Paren,     // ( 
        Right_Paren,    // )
        Left_Brace,     // {
        Right_Brace,    // }
        Comma,          // ,
        Dot,            // .
        Minus,          // -
        Plus,           // +
        Semicolon,      // ;
        At,             // @
        Slash,          // /
        Star,           // *

        // =========一个或两个字符组成的Token=============
        Bang,           // !
        Bang_Equal,     // !=
        Equal,          // =
        Equal_Equal,    // ==
        Greater,        // >
        Greater_Equal,  // >=
        Less,           // <
        Less_Equal,     // <=
        //字面量（Literals）
        Identifier,     // 标识符
        Strings,        // 字符串
        Number,         // 数字

        //关键字(KeyWord)

        And,
        Else,
        False,
        For,
        If,
        Nil,
        Or,
        True,
        Var,
        While,
        //结尾符号  
        EOF             // /0,End Of File
    }
    public struct Token
    {
        public TokenType type;
        public string sourceString;
        public object literal;
        public Token(TokenType type, string sourceString, object literal)
        {
            this.literal = literal;
            this.type = type;
            this.sourceString = sourceString;
        }
    }

}

using System.Collections.Generic;
using UnityEngine;
namespace Helper
{
    public class Scanner
    {
        Dictionary<string, TokenType> keywords;
        List<Token> tokens = new();
        string source;
        int line;
        int start = 0;
        int current = 0;
        bool IsEnd => current >= source.Length;
        public Scanner()
        {
            keywords = new()
            {
                { "else", TokenType.Else },
                { "false", TokenType.False },
                { "true", TokenType.True },
                { "for", TokenType.For },
                { "if", TokenType.If },
                { "||", TokenType.Or },
                { "&&",TokenType.And},
                { "nil", TokenType.Nil },
                { "var", TokenType.Var },
                { "while", TokenType.While },
            };
        }

        public List<Token> Parser(string source)
        {
            this.source = source;
            current = start = 0;
            tokens.Clear();

            while (!IsEnd)
            {
                //每次扫描一个Token以后会调整Start下标
                start = current;
                ScanToken();
            }
            tokens.Add(new Token(TokenType.EOF, "", null));
            return tokens;
        }

        void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(TokenType.Left_Paren);
                    break;
                case ')':
                    AddToken(TokenType.Right_Paren);
                    break;
                case '{':
                    AddToken(TokenType.Left_Brace);
                    break;
                case '}':
                    AddToken(TokenType.Right_Brace);
                    break;
                case ',':
                    AddToken(TokenType.Comma);
                    break;
                case '.':
                    AddToken(TokenType.Dot);
                    break;
                case '-':
                    AddToken(TokenType.Minus);
                    break;
                case '+':
                    AddToken(TokenType.Plus);
                    break;
                case ';':
                    AddToken(TokenType.Semicolon);
                    break;
                case '*':
                    AddToken(TokenType.Star);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.Bang_Equal : TokenType.Bang);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.Less_Equal : TokenType.Less);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.Equal_Equal : TokenType.Equal);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.Greater_Equal : TokenType.Greater);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        //如果是 //的情况，那么就一直推进到本行的结尾，即一直读取到 结束或者换行符
                        while (Peek() != '\n' && !IsEnd)
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                // 或许我们可以在之前就剔除所有的空格与特殊字符
                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    line++;
                    break;
                case '"':
                    NextString();
                    break;
                default:
                    if (IsDigit(c))
                    {
                        NextNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        NextIdentifier();
                    }
                    else if (c == '&' && Match('&'))
                    {
                        AddToken(TokenType.And);
                    }
                    else if (c == '|' && Match('|'))
                    {
                        AddToken(TokenType.Or);
                    }
                    else
                    {
                        //Program.Error(line, "Unexpected character");
                    }
                    break;
            }
        }
        void NextNumber()
        {
            //这里之所以先用Peek是因为，当我们读取到了非数字的时候不应该读取字符
            //而如果直接使用Advance会导致最后一个数字的下一个字符被读取
            while (IsDigit(Peek()))
            {
                Advance();
            }
            //小数部分
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                //越过小数点，
                Advance();
                //继续推进
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }
            AddToken(TokenType.Number, float.Parse(source.Substring(start, current - start)));
        }
        /// <summary>
        /// 转化为标识符,从首个可识别的标识符开始，
        /// 直到遇到非字母或数字
        /// </summary>
        void NextIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
                Advance();
            string text = source.Substring(start, current - start);

            if (!keywords.TryGetValue(text, out var type))
            {
                type = TokenType.Identifier;
            }
            AddToken(type);
        }
        bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
        /// <summary>
        /// 处理数字
        /// </summary>
        /// <returns></returns>
        bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// 判断是否是合法的字母
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z'
                || c >= 'A' && c <= 'Z'
                || c == '_';
        }
        void NextString()
        {
            //当我们读取到了一 个 “，就会尝试读取下一个，但是不推进
            //一个一个推进直到文件末尾
            while (Peek() != '"' && !IsEnd)
            {
                if (Peek() == '\n')
                    line++;
                Advance();
            }
            if (IsEnd)
            {
                //Program.Error(line, "Unterminated string");
                return;
            }
            ///越过 ",比如 "abc"中c中后面的引号
            Advance();

            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.Strings, value);

        }

        #region Utilities
        /// <summary>
        /// 返回字符,并向前推进一个字符
        /// </summary>
        /// <returns></returns>
        char Advance()
        {
            current++;
            return source[current - 1];
        }

        /// <summary>
        /// 判断字符是不是我们想要的字符
        /// 如果是则会推进
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        bool Match(char c)
        {
            if (IsEnd)
                return false;
            if (source[current] != c)
            {
                return false;
            }
            current++;
            return true;
        }

        /// <summary>
        /// 获得字符，但不推进
        /// </summary>
        /// <returns></returns>
        char Peek()
        {
            if (IsEnd)
            {
                return '\0';
            }
            else return source[current];
        }
        void AddToken(TokenType type)
        {
            AddToken(type, null);
        }
        void AddToken(TokenType type, object literal)
        {
            string text = source[start..current];
            tokens.Add(new Token(type, text, literal));
        }
        /// <summary>
        /// 获得下个字符，依然是不推进
        /// </summary>
        /// <returns></returns>
        char PeekNext()
        {
            if (IsEnd)
            {
                return '\0';
            }
            else return source[current + 1];
        }
        #endregion
    }
}

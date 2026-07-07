using System;
using System.Collections.Generic;
using System.Globalization;

namespace Helper
{
    public enum TokenType
    {
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Semicolon,
        Slash,
        Star,
        Bang,
        BangEqual,
        Equal,
        EqualEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        At,
        Identifier,
        String,
        Number,
        And,
        Else,
        False,
        Fun,
        If,
        Nil,
        Or,
        Return,
        True,
        Var,
        While,
        Error,
        EOF
    }

    public readonly struct Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object Literal;
        public readonly int Line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }

    public sealed class Scanner
    {
        static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.Ordinal)
        {
            { "and", TokenType.And },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "fun", TokenType.Fun },
            { "if", TokenType.If },
            { "nil", TokenType.Nil },
            { "or", TokenType.Or },
            { "return", TokenType.Return },
            { "true", TokenType.True },
            { "var", TokenType.Var },
            { "while", TokenType.While }
        };

        readonly List<Token> tokens = new();
        string source;
        int start;
        int current;
        int line;

        bool IsAtEnd => current >= source.Length;

        public List<Token> ScanTokens(string sourceText)
        {
            source = sourceText ?? string.Empty;
            start = 0;
            current = 0;
            line = 1;
            tokens.Clear();

            while (!IsAtEnd)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, string.Empty, null, line));
            return new List<Token>(tokens);
        }

        void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(TokenType.LeftParen);
                    break;
                case ')':
                    AddToken(TokenType.RightParen);
                    break;
                case '{':
                    AddToken(TokenType.LeftBrace);
                    break;
                case '}':
                    AddToken(TokenType.RightBrace);
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
                case '@':
                    AddToken(TokenType.At);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd)
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                case '&':
                    if (Match('&'))
                    {
                        AddToken(TokenType.And);
                    }
                    else
                    {
                        AddError("需要使用 && 表示逻辑与");
                    }
                    break;
                case '|':
                    if (Match('|'))
                    {
                        AddToken(TokenType.Or);
                    }
                    else
                    {
                        AddError("需要使用 || 表示逻辑或");
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                case '"':
                    ScanString();
                    break;
                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        AddError($"无法识别的字符 {c}");
                    }
                    break;
            }
        }

        void ScanString()
        {
            while (Peek() != '"' && !IsAtEnd)
            {
                if (Peek() == '\n')
                    line++;
                Advance();
            }

            if (IsAtEnd)
            {
                AddError("字符串缺少结尾引号");
                return;
            }

            Advance();
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.String, value);
        }

        void ScanNumber()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            string text = source.Substring(start, current - start);
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                AddToken(TokenType.Number, value);
                return;
            }

            AddError($"数字格式错误 {text}");
        }

        void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            string text = source.Substring(start, current - start);
            if (!Keywords.TryGetValue(text, out TokenType type))
            {
                type = TokenType.Identifier;
            }

            AddToken(type);
        }

        char Advance()
        {
            current++;
            return source[current - 1];
        }

        bool Match(char expected)
        {
            if (IsAtEnd)
                return false;
            if (source[current] != expected)
                return false;
            current++;
            return true;
        }

        char Peek()
        {
            return IsAtEnd ? '\0' : source[current];
        }

        char PeekNext()
        {
            return current + 1 >= source.Length ? '\0' : source[current + 1];
        }

        void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        void AddError(string message)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(TokenType.Error, text, message, line));
        }

        static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        static bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z'
                || c >= 'A' && c <= 'Z'
                || c == '_';
        }

        static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}

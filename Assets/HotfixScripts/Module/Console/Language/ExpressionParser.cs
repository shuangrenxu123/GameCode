using System.Collections.Generic;
using UnityEngine;
namespace Helper
{
    public class ExpressionParser
    {
        private List<Token> tokens;
        /// <summary>
        /// 待解析token下标
        /// </summary>
        private int current = 0;
        private bool isEnd => current >= tokens.Count || tokens[current].type == TokenType.EOF;


        public void Init(List<Token> tokens)
        {
            current = 0;
            this.tokens = tokens;
        }
        public Expression Expression()
        {
            return Assignment();
        }
        private Expression Assignment()
        {
            Expression left = Or();
            if (Match(TokenType.Equal))
            {
                //形成一个赋值的语法树
                Expression value = Assignment();

                if (left is VariableExpression variable)
                {
                    Token name = variable.name;
                    return new AssignExpression(name, value);
                }
                if (left is ExternalVariableExpression externalVariable)
                {
                    return new AssignExpression(externalVariable, value);
                }

                throw new RuntimeException(Previous(), "无效的赋值目标");
            }
            return left;
        }
        private Expression Or()
        {
            Expression left = And();
            while (Match(TokenType.Or))
            {
                Token opt = Previous();
                Expression right = And();
                left = new LogicalExpression(left, opt, right);
            }
            return left;
        }
        private Expression And()
        {
            Expression left = Equality();
            while (Match(TokenType.And))
            {
                Token opt = Previous();
                Expression right = Equality();
                left = new LogicalExpression(left, opt, right);
            }
            return left;
        }
        private Expression Equality()
        {
            Expression left = Comparison();
            while (Match(TokenType.Bang_Equal, TokenType.Equal_Equal))
            {

                Token opt = Previous();
                Expression right = Comparison();
                left = new Binary(left, opt, right);
            }

            return left;
        }

        /// <summary>
        /// 比较运算符 > >= < <=
        /// </summary>
        /// <returns></returns>
        private Expression Comparison()
        {
            Expression left = Term();

            while (Match(TokenType.Greater_Equal, TokenType.Greater,
                    TokenType.Less, TokenType.Less_Equal))
            {
                Token opt = Previous();
                Expression right = Term();
                left = new Binary(left, opt, right);
            }
            return left;
        }

        /// <summary>
        /// 加减
        /// </summary>
        /// <returns></returns>
        private Expression Term()
        {
            Expression left = Factor();
            while (Match(TokenType.Minus, TokenType.Plus))
            {
                Token opt = Previous();
                Expression right = Factor();
                left = new Binary(left, opt, right);
            }
            return left;
        }
        /// <summary>
        /// 乘除
        /// </summary>
        /// <returns></returns>
        private Expression Factor()
        {
            Expression left = Unary();
            while (Match(TokenType.Slash, TokenType.Star))
            {
                Token opt = Previous();
                Expression right = Unary();
                left = new Binary(left, opt, right);
            }

            return left;
        }
        /// <summary>
        /// 一元运算符
        /// </summary>
        /// <returns></returns>
        private Expression Unary()
        {
            if (Match(TokenType.Bang, TokenType.Bang_Equal))
            {
                Token opt = Previous();
                Expression right = Unary();
                return new UnaryExpression(opt, right);
            }
            return Call();
        }
        private Expression Call()
        {
            Expression expr = Primary();
            while (true)
            {
                if (Match(TokenType.Left_Paren))
                {
                    expr = FinishCall(expr);
                    continue;
                }

                if (IsBareCallCandidate(expr) && CanStartBareCallArguments())
                {
                    expr = FinishBareCall(expr);
                    continue;
                }

                break;
            }
            return expr;
        }

        /// <summary>
        /// 返回包装后的版本
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        Expression FinishCall(Expression call)
        {
            List<Expression> args = new List<Expression>();
            if (!CheckCurrentTokenType(TokenType.Right_Paren))
            {
                do
                {
                    args.Add(Expression());
                }
                while (Match(TokenType.Comma));
            }

            Token paren = Consume(TokenType.Right_Paren, "函数调用需要右括号结尾");
            return new CallExpression(call, paren, args);
        }

        Expression FinishBareCall(Expression call)
        {
            List<Expression> args = new List<Expression>();
            while (!isEnd && IsExpressionStart(Peek().type))
            {
                args.Add(Expression());
                Match(TokenType.Comma);
            }

            return new CallExpression(call, new Token(), args);
        }
        /// <summary>
        /// 最终值
        /// </summary>
        /// <returns></returns>
        private Expression Primary()
        {
            var token = Advance();
            Expression result;
            result = token.type switch
            {
                TokenType.False => new LiteralExpression(false),
                TokenType.True => new LiteralExpression(true),
                TokenType.Nil => new LiteralExpression(null),
                TokenType.Number or TokenType.Strings => new LiteralExpression(Previous().literal),
                _ => null
            };

            if (token.type == TokenType.Identifier)
            {
                if (CheckCurrentTokenType(TokenType.Left_Paren))
                {
                    return new LiteralExpression(token.sourceString);
                }
                if (IsNextTokenBareCallArgument())
                {
                    return new LiteralExpression(token.sourceString);
                }
                return new VariableExpression(token);
            }

            if (token.type == TokenType.At)
            {
                Token variableName = ConsumeIdentifier("@语法需要紧跟变量名");
                List<Token> accessChain = ParseAccessChain();
                return new ExternalVariableExpression(variableName, accessChain);
            }

            if (token.type == TokenType.Left_Paren)
            {
                Expression expr = Expression();
                Consume(TokenType.Right_Paren, "没有识别到右括号");
                return new GroupExpression(expr);
            }


            if (result == null)
            {
                throw new RuntimeException(token, "识别到未知的类型");
            }

            return result;
        }

        #region  Utilities
        /// <summary>
        /// 如果当前Token类型匹配则取出并推进
        /// </summary>
        private bool Match(params TokenType[] tokens)
        {
            foreach (TokenType type in tokens)
            {
                if (CheckCurrentTokenType(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 取出并推进一个Token
        /// </summary>
        /// <returns></returns>
        private Token Advance()
        {
            if (!isEnd)
                current++;
            return Previous();
        }

        private Token Peek()
        {
            return tokens[current];
        }
        /// <summary>
        /// 取出上一个访问过的Token,但是不推进
        /// </summary>
        private Token Previous()
        {
            return tokens[current - 1];
        }
        /// <summary>
        /// 检测当前待使用的Tokentype，如果不匹配则报错
        /// </summary>
        /// <param name="type"></param>
        /// <param name="errMessage"></param>
        private Token Consume(TokenType type, string errMessage)
        {
            if (CheckCurrentTokenType(type))
            {
                return Advance();
            }
            else
            {
                return new Token();
            }
        }
        /// <summary>
        /// 检查当前的Token类型是否相同
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool CheckCurrentTokenType(TokenType type)
        {
            if (isEnd)
                return false;
            return type == Peek().type;
        }

        private List<Token> ParseAccessChain()
        {
            List<Token> chain = new();
            while (Match(TokenType.Dot))
            {
                chain.Add(ConsumeIdentifier("点号后需要标识符"));
            }
            return chain;
        }

        private Token ConsumeIdentifier(string errMessage)
        {
            if (!CheckCurrentTokenType(TokenType.Identifier))
            {
                Token errorToken = isEnd ? Previous() : Peek();
                throw new RuntimeException(errorToken, errMessage);
            }
            return Advance();
        }

        bool IsBareCallCandidate(Expression expr)
        {
            if (expr is LiteralExpression literal && literal.value is string)
                return true;
            if (expr is ExternalVariableExpression)
                return true;
            return false;
        }

        bool CanStartBareCallArguments()
        {
            if (isEnd)
                return false;
            return IsExpressionStart(Peek().type);
        }

        bool IsNextTokenBareCallArgument()
        {
            if (isEnd)
                return false;
            return IsExpressionStart(Peek().type);
        }

        bool IsExpressionStart(TokenType type)
        {
            return type == TokenType.Identifier
                || type == TokenType.Number
                || type == TokenType.Strings
                || type == TokenType.Left_Paren
                || type == TokenType.At
                || type == TokenType.True
                || type == TokenType.False
                || type == TokenType.Nil
                || type == TokenType.Minus
                || type == TokenType.Bang;
        }
        #endregion
    }
}
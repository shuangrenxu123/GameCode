using System.Collections.Generic;
using UnityEngine;
namespace Helper
{
    public class ExpressionParser
    {
        private readonly List<Token> tokens;
        /// <summary>
        /// 待解析token下标
        /// </summary>
        private int current = 0;
        private bool isEnd => tokens[current].type == TokenType.EOF;


        public ExpressionParser(List<Token> tokens)
        {
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

                if (left is VariableExpression)
                {
                    VariableExpression variable = left as VariableExpression;
                    Token name = variable.name;
                    return new AssignExpression(name, value);
                }
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
                }
                else
                {
                    break;
                }
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
                return new VariableExpression(token);
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
        #endregion
    }
}
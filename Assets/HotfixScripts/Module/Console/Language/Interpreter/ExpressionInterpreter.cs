using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public class ExpressionInterpreter : Expression.IVisitor<object>
    {
        //public Environment globals = new Environment();
        private Environment environment = new();
        public object Interpret(Expression expr)
        {
            try
            {
                object value = Evaluate(expr);
                return value;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public object VisitBinaryExpr(Binary expr)
        {
            var left = Evaluate(expr.left);
            var right = Evaluate(expr.right);

            if (expr.opt.type == TokenType.Minus)
            {
                checkNumberOperand(expr.opt, left, right);
                return (float)left - (float)right;
            }

            if (expr.opt.type == TokenType.Plus)
            {
                if ((left is float) && (right is float))
                    return (float)left + (float)right;

                else if (left is string && right is float)
                    return (string)left + right.ToString();

                else if (left is float && right is string)
                    return ((float)left).ToString() + (string)right;

                else if (left is string && right is string)
                    return (string)left + (string)right;

                else
                {
                    // throw new RuntimeError(expr.opt, "操作类型错误");
                }
            }

            if (expr.opt.type == TokenType.Star)
            {
                checkNumberOperand(expr.opt, left, right);
                return (float)left * (float)right;
            }
            if (expr.opt.type == TokenType.Slash)
            {
                checkNumberOperand(expr.opt, left, right);
                return (float)left / (float)right;
            }

            switch (expr.opt.type)
            {
                case TokenType.Greater:
                    checkNumberOperand(expr.opt, left, right);
                    return (float)left > (float)right;
                case TokenType.Greater_Equal:
                    checkNumberOperand(expr.opt, left, right);
                    return (float)left >= (float)right;
                case TokenType.Less:
                    checkNumberOperand(expr.opt, left, right);
                    return (float)left < (float)right;
                case TokenType.Less_Equal:
                    checkNumberOperand(expr.opt, left, right);
                    return (float)left <= (float)right;
            }

            if (expr.opt.type == TokenType.Bang_Equal)
            {
                return !IsEqual(left, right);
            }
            if (expr.opt.type == TokenType.Equal_Equal)
            {
                return IsEqual(left, right);
            }

            return null;
        }

        public object VisitGroupingExpr(GroupExpression expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(LiteralExpression expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(UnaryExpression expr)
        {
            object result = Evaluate(expr.right);

            if (expr.opt.type == TokenType.Minus)
            {
                return -(float)result;
            }

            if (expr.opt.type == TokenType.Bang)
            {
                return !IsTruthy(result);
            }

            return null;
        }

        private void checkNumberOperand(Token opt, params object[] objs)
        {
            foreach (var i in objs)
            {
                if (i is not float)
                { }
                // throw new RuntimeError(opt, "类型错误");
            }
        }
        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            return left.Equals(right);
        }

        private bool IsTruthy(object value)
        {
            if (value == null)
                return false;
            if (value is bool)
            {
                return (bool)value;
            }
            return true;
        }

        /// <summary>
        /// 执行表达式
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private object Evaluate(Expression expr)
        {
            return expr.Accept(this);
        }

        /// <summary>
        /// 变量表达式
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitVariableExpr(VariableExpression expr)
        {
            return environment.GetVariables(expr.name);
        }
        /// <summary>
        /// 赋值语句的执行
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitAssignExpr(AssignExpression expr)
        {
            object value = Evaluate(expr.right);
            environment.Assign(expr.name, value);
            //将最右侧的值传递过去
            return value;
        }

        public object VisitLogicalExpr(LogicalExpression expr)
        {
            object left = Evaluate(expr.left);
            if (expr.opt.type == TokenType.Or)
            {
                if (IsTruthy(left))
                    return left;
            }
            else
            {
                if (!IsTruthy(left))
                    return left;
            }
            return Evaluate(expr.right);
        }

        public object VisitCallExpr(CallExpression expr)
        {
            var prams = new List<string>();
            foreach (var arg in expr.args)
            {
                prams.Add(Evaluate(arg).ToString());
            }
            var methodName = Evaluate(expr.callee) as string;
            var method = environment.GetMethod(methodName);
            method.Execute(prams);
            return null;
        }
    }
}

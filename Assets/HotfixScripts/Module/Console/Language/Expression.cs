using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public abstract class Expression
    {

        public interface IVisitor<T>
        {
            public T VisitBinaryExpr(Binary expr);
            public T VisitAssignExpr(AssignExpression expr);
            public T VisitGroupingExpr(GroupExpression expr);
            public T VisitLiteralExpr(LiteralExpression expr);
            public T VisitUnaryExpr(UnaryExpression expr);
            public T VisitLogicalExpr(LogicalExpression expr);
            public T VisitVariableExpr(VariableExpression expr);
            public T VisitCallExpr(CallExpression expr);
            public T VisitExternalVariableExpr(ExternalVariableExpression expr);
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
    public class Binary : Expression
    {
        public Expression left;
        public Token opt;
        public Expression right;
        public Binary(Expression left, Token opt, Expression right)
        {
            this.left = left;
            this.opt = opt;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }
    public class AssignExpression : Expression
    {
        public Token name;
        public ExternalVariableExpression externalTarget;
        public Expression right;
        public bool IsExternal => externalTarget != null;
        public AssignExpression(Token name, Expression right)
        {
            this.name = name;
            this.right = right;
        }

        public AssignExpression(ExternalVariableExpression externalTarget, Expression right)
        {
            this.externalTarget = externalTarget;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitAssignExpr(this);
        }
    }
    public class GroupExpression : Expression
    {
        public Expression expression;
        public GroupExpression(Expression expression)
        {
            this.expression = expression;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }
    public class LiteralExpression : Expression
    {
        public object value;
        public LiteralExpression(object value)
        {
            this.value = value;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }
    public class UnaryExpression : Expression
    {
        public Token opt;
        public Expression right;
        public UnaryExpression(Token opt, Expression right)
        {
            this.opt = opt;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }
    public class LogicalExpression : Expression
    {
        public Expression left;
        public Token opt;
        public Expression right;
        public LogicalExpression(Expression left, Token opt, Expression right)
        {
            this.left = left;
            this.opt = opt;
            this.right = right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpr(this);
        }
    }
    public class VariableExpression : Expression
    {
        public Token name;
        public VariableExpression(Token name)
        {
            this.name = name;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableExpr(this);
        }
    }
    public class ExternalVariableExpression : Expression
    {
        public Token root;
        public List<Token> accessChain;
        public ExternalVariableExpression(Token root, List<Token> accessChain)
        {
            this.root = root;
            this.accessChain = accessChain ?? new List<Token>();
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExternalVariableExpr(this);
        }
    }
    public class CallExpression : Expression
    {
        public Expression callee;
        public Token paren;
        public List<Expression> args;
        public CallExpression(Expression callee, Token paren, List<Expression> args)
        {
            this.callee = callee;
            this.paren = paren;
            this.args = args;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitCallExpr(this);
        }
    }

}

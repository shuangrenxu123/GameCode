using System;
using System.Collections.Generic;

namespace Helper
{
    public readonly struct CompileResult
    {
        public readonly bool Success;
        public readonly RuntimeFunction Function;
        public readonly string Error;

        CompileResult(bool success, RuntimeFunction function, string error)
        {
            Success = success;
            Function = function;
            Error = error;
        }

        public static CompileResult Ok(RuntimeFunction function)
        {
            return new CompileResult(true, function, null);
        }

        public static CompileResult Fail(string error)
        {
            return new CompileResult(false, null, error);
        }
    }

    enum Precedence
    {
        None,
        Assignment,
        Or,
        And,
        Equality,
        Comparison,
        Term,
        Factor,
        Unary,
        Call,
        Primary
    }

    readonly struct ParseRule
    {
        public readonly Action<bool> Prefix;
        public readonly Action<bool> Infix;
        public readonly Precedence Precedence;

        public ParseRule(Action<bool> prefix, Action<bool> infix, Precedence precedence)
        {
            Prefix = prefix;
            Infix = infix;
            Precedence = precedence;
        }
    }

    readonly struct Local
    {
        public readonly InternedString Name;
        public readonly int Depth;

        public Local(InternedString name, int depth)
        {
            Name = name;
            Depth = depth;
        }
    }

    sealed class FunctionCompiler
    {
        public readonly RuntimeFunction Function;
        public readonly FunctionCompiler Enclosing;
        public readonly List<Local> Locals = new();
        public int ScopeDepth;

        public FunctionCompiler(RuntimeFunction function, FunctionCompiler enclosing)
        {
            Function = function;
            Enclosing = enclosing;
        }
    }

    public sealed class Compiler
    {
        readonly Scanner scanner = new();
        readonly StringTable strings;
        List<Token> tokens;
        int current;
        bool hadError;
        bool panicMode;
        readonly List<string> errors = new();
        FunctionCompiler currentCompiler;

        public Compiler(StringTable strings)
        {
            this.strings = strings ?? throw new ArgumentNullException(nameof(strings));
        }

        public CompileResult Compile(string source)
        {
            tokens = scanner.ScanTokens(source);
            current = 0;
            hadError = false;
            panicMode = false;
            errors.Clear();

            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.Error)
                {
                    ErrorAt(token, token.Literal?.ToString() ?? "扫描错误");
                }
            }

            RuntimeFunction script = new("<script>");
            currentCompiler = new FunctionCompiler(script, null);

            while (!Check(TokenType.EOF))
            {
                Declaration();
            }

            EmitNilReturn();
            currentCompiler = null;

            if (hadError)
            {
                return CompileResult.Fail(string.Join("\n", errors));
            }

            return CompileResult.Ok(script);
        }

        void Declaration()
        {
            if (Match(TokenType.Fun))
            {
                FunctionDeclaration();
            }
            else if (Match(TokenType.Var))
            {
                VariableDeclaration();
            }
            else
            {
                Statement();
            }

            if (panicMode)
            {
                Synchronize();
            }
        }

        void FunctionDeclaration()
        {
            Token nameToken = ConsumeIdentifier("函数声明需要函数名");
            InternedString name = strings.Intern(nameToken.Lexeme);
            CompileFunction(name, nameToken.Line);

            if (currentCompiler.ScopeDepth == 0)
            {
                Emit(OpCode.DefineGlobal, new NameOperand(name), nameToken.Line);
            }
            else
            {
                AddLocal(name, nameToken.Line);
            }
        }

        void CompileFunction(InternedString name, int line)
        {
            RuntimeFunction function = new(name.Value);
            FunctionCompiler enclosing = currentCompiler;
            currentCompiler = new FunctionCompiler(function, enclosing);

            Consume(TokenType.LeftParen, "函数参数列表需要左括号");
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (function.Arity >= 255)
                    {
                        ErrorAtCurrent("函数最多支持 255 个参数");
                    }

                    Token parameter = ConsumeIdentifier("函数参数需要名字");
                    InternedString parameterName = strings.Intern(parameter.Lexeme);
                    AddLocal(parameterName, parameter.Line);
                    function.Arity++;
                }
                while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "函数参数列表需要右括号");
            Consume(TokenType.LeftBrace, "函数体需要左大括号");
            BeginScope();
            BlockBody();
            EndScope();
            EmitNilReturn();

            currentCompiler = enclosing;
            EmitConstant(function, line);
        }

        void VariableDeclaration()
        {
            Token nameToken = ConsumeIdentifier("变量声明需要变量名");
            InternedString name = strings.Intern(nameToken.Lexeme);

            if (Match(TokenType.Equal))
            {
                Expression();
            }
            else
            {
                Emit(OpCode.Nil, nameToken.Line);
            }

            ConsumeStatementEnd("变量声明后需要分号");

            if (currentCompiler.ScopeDepth == 0)
            {
                Emit(OpCode.DefineGlobal, new NameOperand(name), nameToken.Line);
            }
            else
            {
                AddLocal(name, nameToken.Line);
            }
        }

        void Statement()
        {
            if (Match(TokenType.If))
            {
                IfStatement();
            }
            else if (Match(TokenType.While))
            {
                WhileStatement();
            }
            else if (Match(TokenType.Return))
            {
                ReturnStatement();
            }
            else if (Match(TokenType.LeftBrace))
            {
                BeginScope();
                BlockBody();
                EndScope();
            }
            else if (IsBareCommandStatement())
            {
                BareCommandStatement();
            }
            else
            {
                ExpressionStatement();
            }
        }

        void IfStatement()
        {
            Consume(TokenType.LeftParen, "if 条件需要左括号");
            Expression();
            Consume(TokenType.RightParen, "if 条件需要右括号");

            int thenJump = EmitJump(OpCode.JumpIfFalse, Previous().Line);
            Emit(OpCode.Pop, Previous().Line);
            Statement();

            int elseJump = EmitJump(OpCode.Jump, Previous().Line);
            PatchJump(thenJump);
            Emit(OpCode.Pop, Previous().Line);

            if (Match(TokenType.Else))
            {
                Statement();
            }

            PatchJump(elseJump);
        }

        void WhileStatement()
        {
            int loopStart = CurrentChunk.Count;
            Consume(TokenType.LeftParen, "while 条件需要左括号");
            Expression();
            Consume(TokenType.RightParen, "while 条件需要右括号");

            int exitJump = EmitJump(OpCode.JumpIfFalse, Previous().Line);
            Emit(OpCode.Pop, Previous().Line);
            Statement();
            Emit(OpCode.Loop, loopStart, Previous().Line);

            PatchJump(exitJump);
            Emit(OpCode.Pop, Previous().Line);
        }

        void ReturnStatement()
        {
            if (currentCompiler.Enclosing == null)
            {
                ErrorAt(Previous(), "顶层代码不能使用 return");
            }

            if (Check(TokenType.Semicolon) || Check(TokenType.RightBrace) || Check(TokenType.EOF))
            {
                Emit(OpCode.Nil, Previous().Line);
            }
            else
            {
                Expression();
            }

            ConsumeStatementEnd("return 后需要分号");
            Emit(OpCode.Return, Previous().Line);
        }

        void BlockBody()
        {
            while (!Check(TokenType.RightBrace) && !Check(TokenType.EOF))
            {
                Declaration();
            }

            Consume(TokenType.RightBrace, "代码块需要右大括号");
        }

        void ExpressionStatement()
        {
            Expression();
            ConsumeStatementEnd("表达式后需要分号");
            Emit(OpCode.Pop, Previous().Line);
        }

        void BareCommandStatement()
        {
            Token commandToken = Advance();
            InternedString commandName = strings.Intern(commandToken.Lexeme);
            int argumentCount = 0;

            while (!Check(TokenType.Semicolon)
                   && !Check(TokenType.RightBrace)
                   && !Check(TokenType.EOF))
            {
                Expression();
                argumentCount++;
                Match(TokenType.Comma);
            }

            ConsumeOptionalSemicolon();
            Emit(OpCode.InvokeCommand, new CommandOperand(commandName, argumentCount), commandToken.Line);
            Emit(OpCode.Pop, commandToken.Line);
        }

        void Expression()
        {
            ParsePrecedence(Precedence.Assignment);
        }

        void ParsePrecedence(Precedence precedence)
        {
            Advance();
            ParseRule prefixRule = GetRule(Previous().Type);
            if (prefixRule.Prefix == null)
            {
                ErrorAt(Previous(), "需要表达式");
                return;
            }

            bool canAssign = precedence <= Precedence.Assignment;
            prefixRule.Prefix(canAssign);

            while (precedence <= GetRule(Peek().Type).Precedence)
            {
                Advance();
                ParseRule infixRule = GetRule(Previous().Type);
                infixRule.Infix(canAssign);
            }

            if (canAssign && Match(TokenType.Equal))
            {
                ErrorAt(Previous(), "无效的赋值目标");
            }
        }

        void Number(bool canAssign)
        {
            EmitConstant(Previous().Literal, Previous().Line);
        }

        void String(bool canAssign)
        {
            EmitConstant(Previous().Literal, Previous().Line);
        }

        void Literal(bool canAssign)
        {
            switch (Previous().Type)
            {
                case TokenType.False:
                    Emit(OpCode.False, Previous().Line);
                    break;
                case TokenType.True:
                    Emit(OpCode.True, Previous().Line);
                    break;
                case TokenType.Nil:
                    Emit(OpCode.Nil, Previous().Line);
                    break;
            }
        }

        void Grouping(bool canAssign)
        {
            Expression();
            Consume(TokenType.RightParen, "分组表达式需要右括号");
        }

        void Unary(bool canAssign)
        {
            TokenType operatorType = Previous().Type;
            ParsePrecedence(Precedence.Unary);

            switch (operatorType)
            {
                case TokenType.Bang:
                    Emit(OpCode.Not, Previous().Line);
                    break;
                case TokenType.Minus:
                    Emit(OpCode.Negate, Previous().Line);
                    break;
            }
        }

        void Binary(bool canAssign)
        {
            TokenType operatorType = Previous().Type;
            ParseRule rule = GetRule(operatorType);
            ParsePrecedence((Precedence)((int)rule.Precedence + 1));

            switch (operatorType)
            {
                case TokenType.Plus:
                    Emit(OpCode.Add, Previous().Line);
                    break;
                case TokenType.Minus:
                    Emit(OpCode.Subtract, Previous().Line);
                    break;
                case TokenType.Star:
                    Emit(OpCode.Multiply, Previous().Line);
                    break;
                case TokenType.Slash:
                    Emit(OpCode.Divide, Previous().Line);
                    break;
                case TokenType.BangEqual:
                    Emit(OpCode.Equal, Previous().Line);
                    Emit(OpCode.Not, Previous().Line);
                    break;
                case TokenType.EqualEqual:
                    Emit(OpCode.Equal, Previous().Line);
                    break;
                case TokenType.Greater:
                    Emit(OpCode.Greater, Previous().Line);
                    break;
                case TokenType.GreaterEqual:
                    Emit(OpCode.GreaterEqual, Previous().Line);
                    break;
                case TokenType.Less:
                    Emit(OpCode.Less, Previous().Line);
                    break;
                case TokenType.LessEqual:
                    Emit(OpCode.LessEqual, Previous().Line);
                    break;
                case TokenType.And:
                    Emit(OpCode.And, Previous().Line);
                    break;
                case TokenType.Or:
                    Emit(OpCode.Or, Previous().Line);
                    break;
            }
        }

        void Variable(bool canAssign)
        {
            NamedVariable(Previous(), canAssign);
        }

        void NamedVariable(Token nameToken, bool canAssign)
        {
            InternedString name = strings.Intern(nameToken.Lexeme);
            int localSlot = ResolveLocal(name);
            OpCode getOp = localSlot >= 0 ? OpCode.GetLocal : OpCode.GetGlobal;
            OpCode setOp = localSlot >= 0 ? OpCode.SetLocal : OpCode.SetGlobal;
            object operand = localSlot >= 0 ? localSlot : new NameOperand(name);

            if (canAssign && Match(TokenType.Equal))
            {
                Expression();
                Emit(setOp, operand, nameToken.Line);
            }
            else
            {
                Emit(getOp, operand, nameToken.Line);
            }
        }

        void ExternalVariable(bool canAssign)
        {
            Token rootToken = ConsumeIdentifier("@ 后需要外部变量名");
            InternedString root = strings.Intern(rootToken.Lexeme);
            List<InternedString> chain = ParseAccessChain();

            if (canAssign && Match(TokenType.Equal))
            {
                if (chain.Count == 0)
                {
                    Expression();
                    Emit(OpCode.SetExternal, new NameOperand(root), rootToken.Line);
                    return;
                }

                Emit(OpCode.GetExternal, new NameOperand(root), rootToken.Line);
                for (int i = 0; i < chain.Count - 1; i++)
                {
                    Emit(OpCode.GetMember, new NameOperand(chain[i]), rootToken.Line);
                }

                Expression();
                Emit(OpCode.SetMember, new NameOperand(chain[^1]), rootToken.Line);
                return;
            }

            if (Check(TokenType.LeftParen) && chain.Count > 0)
            {
                Advance();
                Emit(OpCode.GetExternal, new NameOperand(root), rootToken.Line);
                for (int i = 0; i < chain.Count - 1; i++)
                {
                    Emit(OpCode.GetMember, new NameOperand(chain[i]), rootToken.Line);
                }

                int argumentCount = FinishArgumentList();
                Emit(OpCode.InvokeMember, new MemberInvokeOperand(chain[^1], argumentCount), rootToken.Line);
                return;
            }

            Emit(OpCode.GetExternal, new NameOperand(root), rootToken.Line);
            foreach (InternedString member in chain)
            {
                Emit(OpCode.GetMember, new NameOperand(member), rootToken.Line);
            }
        }

        void Call(bool canAssign)
        {
            int argumentCount = FinishArgumentList();
            Emit(OpCode.Call, argumentCount, Previous().Line);
        }

        int FinishArgumentList()
        {
            int argumentCount = 0;
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (argumentCount >= 255)
                    {
                        ErrorAtCurrent("调用最多支持 255 个参数");
                    }

                    Expression();
                    argumentCount++;
                }
                while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "调用参数需要右括号");
            return argumentCount;
        }

        List<InternedString> ParseAccessChain()
        {
            List<InternedString> chain = new();
            while (Match(TokenType.Dot))
            {
                Token member = ConsumeIdentifier("点号后需要成员名");
                chain.Add(strings.Intern(member.Lexeme));
            }

            return chain;
        }

        ParseRule GetRule(TokenType type)
        {
            return type switch
            {
                TokenType.LeftParen => new ParseRule(Grouping, Call, Precedence.Call),
                TokenType.Minus => new ParseRule(Unary, Binary, Precedence.Term),
                TokenType.Plus => new ParseRule(null, Binary, Precedence.Term),
                TokenType.Slash => new ParseRule(null, Binary, Precedence.Factor),
                TokenType.Star => new ParseRule(null, Binary, Precedence.Factor),
                TokenType.Bang => new ParseRule(Unary, null, Precedence.None),
                TokenType.BangEqual => new ParseRule(null, Binary, Precedence.Equality),
                TokenType.EqualEqual => new ParseRule(null, Binary, Precedence.Equality),
                TokenType.Greater => new ParseRule(null, Binary, Precedence.Comparison),
                TokenType.GreaterEqual => new ParseRule(null, Binary, Precedence.Comparison),
                TokenType.Less => new ParseRule(null, Binary, Precedence.Comparison),
                TokenType.LessEqual => new ParseRule(null, Binary, Precedence.Comparison),
                TokenType.Identifier => new ParseRule(Variable, null, Precedence.None),
                TokenType.String => new ParseRule(String, null, Precedence.None),
                TokenType.Number => new ParseRule(Number, null, Precedence.None),
                TokenType.And => new ParseRule(null, Binary, Precedence.And),
                TokenType.Or => new ParseRule(null, Binary, Precedence.Or),
                TokenType.False => new ParseRule(Literal, null, Precedence.None),
                TokenType.True => new ParseRule(Literal, null, Precedence.None),
                TokenType.Nil => new ParseRule(Literal, null, Precedence.None),
                TokenType.At => new ParseRule(ExternalVariable, null, Precedence.None),
                _ => new ParseRule(null, null, Precedence.None)
            };
        }

        bool IsBareCommandStatement()
        {
            if (!Check(TokenType.Identifier))
                return false;

            Token next = PeekNextToken();
            if (next.Type == TokenType.LeftParen || next.Type == TokenType.Equal)
                return false;

            if (next.Type == TokenType.Semicolon
                || next.Type == TokenType.RightBrace
                || next.Type == TokenType.EOF)
            {
                return true;
            }

            return IsExpressionStart(next.Type);
        }

        bool IsExpressionStart(TokenType type)
        {
            return type == TokenType.Identifier
                || type == TokenType.String
                || type == TokenType.Number
                || type == TokenType.False
                || type == TokenType.True
                || type == TokenType.Nil
                || type == TokenType.At
                || type == TokenType.LeftParen
                || type == TokenType.Bang
                || type == TokenType.Minus;
        }

        void BeginScope()
        {
            currentCompiler.ScopeDepth++;
        }

        void EndScope()
        {
            currentCompiler.ScopeDepth--;
            while (currentCompiler.Locals.Count > 0
                   && currentCompiler.Locals[^1].Depth > currentCompiler.ScopeDepth)
            {
                Emit(OpCode.Pop, Previous().Line);
                currentCompiler.Locals.RemoveAt(currentCompiler.Locals.Count - 1);
            }
        }

        void AddLocal(InternedString name, int line)
        {
            for (int i = currentCompiler.Locals.Count - 1; i >= 0; i--)
            {
                Local local = currentCompiler.Locals[i];
                if (local.Depth != currentCompiler.ScopeDepth)
                    break;
                if (local.Name.Equals(name))
                {
                    ErrorAtPrevious($"当前作用域已存在变量 {name.Value}");
                    return;
                }
            }

            currentCompiler.Locals.Add(new Local(name, currentCompiler.ScopeDepth));
        }

        int ResolveLocal(InternedString name)
        {
            for (int i = currentCompiler.Locals.Count - 1; i >= 0; i--)
            {
                if (currentCompiler.Locals[i].Name.Equals(name))
                {
                    return i;
                }
            }

            return -1;
        }

        void EmitConstant(object value, int line)
        {
            int constant = CurrentChunk.AddConstant(value);
            Emit(OpCode.Constant, constant, line);
        }

        void EmitNilReturn()
        {
            Emit(OpCode.Nil, PreviousOrCurrentLine());
            Emit(OpCode.Return, PreviousOrCurrentLine());
        }

        void Emit(OpCode code, int line)
        {
            CurrentChunk.Write(code, line);
        }

        void Emit(OpCode code, object operand, int line)
        {
            CurrentChunk.Write(code, operand, line);
        }

        int EmitJump(OpCode code, int line)
        {
            return CurrentChunk.Write(code, null, line);
        }

        void PatchJump(int instructionIndex)
        {
            CurrentChunk.PatchOperand(instructionIndex, CurrentChunk.Count);
        }

        Chunk CurrentChunk => currentCompiler.Function.Chunk;

        bool Match(TokenType type)
        {
            if (!Check(type))
                return false;
            Advance();
            return true;
        }

        bool Check(TokenType type)
        {
            return Peek().Type == type;
        }

        Token Advance()
        {
            if (!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        Token Peek()
        {
            return tokens[current];
        }

        Token PeekNextToken()
        {
            int next = current + 1;
            return next >= tokens.Count ? tokens[^1] : tokens[next];
        }

        Token Previous()
        {
            return tokens[Math.Max(0, current - 1)];
        }

        int PreviousOrCurrentLine()
        {
            return current > 0 ? Previous().Line : Peek().Line;
        }

        Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            ErrorAtCurrent(message);
            return new Token(type, string.Empty, null, Peek().Line);
        }

        Token ConsumeIdentifier(string message)
        {
            return Consume(TokenType.Identifier, message);
        }

        void ConsumeStatementEnd(string message)
        {
            if (Match(TokenType.Semicolon))
                return;
            if (Check(TokenType.EOF) || Check(TokenType.RightBrace))
                return;
            ErrorAtCurrent(message);
        }

        void ConsumeOptionalSemicolon()
        {
            Match(TokenType.Semicolon);
        }

        void ErrorAtCurrent(string message)
        {
            ErrorAt(Peek(), message);
        }

        void ErrorAtPrevious(string message)
        {
            ErrorAt(Previous(), message);
        }

        void ErrorAt(Token token, string message)
        {
            if (panicMode)
                return;

            panicMode = true;
            hadError = true;

            string where = token.Type switch
            {
                TokenType.EOF => "结尾",
                TokenType.Error => token.Lexeme,
                _ => token.Lexeme
            };
            errors.Add($"[line {token.Line}] 在 {where} 附近: {message}");
        }

        void Synchronize()
        {
            panicMode = false;
            while (!Check(TokenType.EOF))
            {
                if (Previous().Type == TokenType.Semicolon)
                    return;

                switch (Peek().Type)
                {
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Return:
                        return;
                }

                Advance();
            }
        }
    }
}

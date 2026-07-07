using System;
using System.Collections.Generic;
using System.Globalization;

namespace Helper
{
    public sealed class RuntimeException : Exception
    {
        public int Line { get; }

        public RuntimeException(string message, int line = 0) : base(message)
        {
            Line = line;
        }
    }

    public readonly struct ExecutionResult
    {
        public readonly bool Success;
        public readonly object Value;
        public readonly string Error;

        ExecutionResult(bool success, object value, string error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public static ExecutionResult Ok(object value)
        {
            return new ExecutionResult(true, value, null);
        }

        public static ExecutionResult Fail(string error)
        {
            return new ExecutionResult(false, null, error);
        }
    }

    public sealed class RuntimeFunction
    {
        public string Name { get; }
        public int Arity { get; set; }
        public Chunk Chunk { get; }

        public RuntimeFunction(string name)
        {
            Name = string.IsNullOrEmpty(name) ? "<script>" : name;
            Chunk = new Chunk();
        }

        public override string ToString()
        {
            return $"<fn {Name}>";
        }
    }

    public enum RuntimeValueKind
    {
        Nil,
        Bool,
        Number,
        String,
        Function,
        Command,
        Object
    }

    public readonly struct RuntimeValue
    {
        readonly double number;
        readonly bool boolean;
        readonly object reference;

        public RuntimeValueKind Kind { get; }
        public double Number => Kind == RuntimeValueKind.Number ? number : 0d;
        public bool Bool => Kind == RuntimeValueKind.Bool && boolean;
        public RuntimeFunction Function => reference as RuntimeFunction;
        public InternedString CommandName => reference as InternedString;

        RuntimeValue(RuntimeValueKind kind, double number, bool boolean, object reference)
        {
            Kind = kind;
            this.number = number;
            this.boolean = boolean;
            this.reference = reference;
        }

        public static RuntimeValue Nil => default;

        public static RuntimeValue FromBool(bool value)
        {
            return new RuntimeValue(RuntimeValueKind.Bool, 0d, value, null);
        }

        public static RuntimeValue FromNumber(double value)
        {
            return new RuntimeValue(RuntimeValueKind.Number, value, false, null);
        }

        public static RuntimeValue FromString(string value)
        {
            return value == null
                ? Nil
                : new RuntimeValue(RuntimeValueKind.String, 0d, false, value);
        }

        public static RuntimeValue FromFunction(RuntimeFunction value)
        {
            return value == null
                ? Nil
                : new RuntimeValue(RuntimeValueKind.Function, 0d, false, value);
        }

        public static RuntimeValue FromCommand(InternedString value)
        {
            return value == null
                ? Nil
                : new RuntimeValue(RuntimeValueKind.Command, 0d, false, value);
        }

        public static RuntimeValue FromObject(object value)
        {
            if (value == null)
                return Nil;
            if (value is RuntimeValue runtimeValue)
                return runtimeValue;
            if (value is bool boolValue)
                return FromBool(boolValue);
            if (IsNumberObject(value))
                return FromNumber(Convert.ToDouble(value, CultureInfo.InvariantCulture));
            if (value is string stringValue)
                return FromString(stringValue);
            if (value is RuntimeFunction function)
                return FromFunction(function);
            if (value is InternedString commandName)
                return FromCommand(commandName);

            return new RuntimeValue(RuntimeValueKind.Object, 0d, false, value);
        }

        public object ToObject()
        {
            return Kind switch
            {
                RuntimeValueKind.Nil => null,
                RuntimeValueKind.Bool => boolean,
                RuntimeValueKind.Number => number,
                RuntimeValueKind.String => reference,
                RuntimeValueKind.Function => reference,
                RuntimeValueKind.Command => reference,
                RuntimeValueKind.Object => reference,
                _ => null
            };
        }

        public bool IsTruthy()
        {
            return Kind switch
            {
                RuntimeValueKind.Nil => false,
                RuntimeValueKind.Bool => boolean,
                _ => true
            };
        }

        public string ToDisplayString()
        {
            return Kind switch
            {
                RuntimeValueKind.Nil => "nil",
                RuntimeValueKind.Bool => boolean ? "true" : "false",
                RuntimeValueKind.Number => number.ToString("0.#######", CultureInfo.InvariantCulture),
                RuntimeValueKind.String => reference?.ToString() ?? string.Empty,
                RuntimeValueKind.Function => reference?.ToString() ?? "nil",
                RuntimeValueKind.Command => $"<command {CommandName?.Value}>",
                RuntimeValueKind.Object => reference?.ToString() ?? "nil",
                _ => "nil"
            };
        }

        public bool EqualsValue(RuntimeValue other)
        {
            if (Kind == RuntimeValueKind.Nil && other.Kind == RuntimeValueKind.Nil)
                return true;
            if (Kind == RuntimeValueKind.Nil || other.Kind == RuntimeValueKind.Nil)
                return false;
            if (Kind == RuntimeValueKind.Number && other.Kind == RuntimeValueKind.Number)
                return Math.Abs(number - other.number) < 0.000001d;
            if (Kind == RuntimeValueKind.Bool && other.Kind == RuntimeValueKind.Bool)
                return boolean == other.boolean;
            if (Kind == RuntimeValueKind.String && other.Kind == RuntimeValueKind.String)
                return string.Equals((string)reference, (string)other.reference, StringComparison.Ordinal);
            if (Kind == RuntimeValueKind.Command && other.Kind == RuntimeValueKind.Command)
                return CommandName.Equals(other.CommandName);

            return Equals(reference, other.reference);
        }

        public override string ToString()
        {
            return ToDisplayString();
        }

        static bool IsNumberObject(object value)
        {
            return value is byte
                || value is sbyte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }
    }

    public struct CallFrame
    {
        public RuntimeFunction Function { get; private set; }
        public int InstructionPointer { get; set; }
        public int SlotStart { get; private set; }
        public int BaseSlot { get; private set; }

        public CallFrame(RuntimeFunction function, int slotStart)
        {
            Reset(function, slotStart, slotStart);
        }

        public void Reset(RuntimeFunction function, int slotStart, int baseSlot)
        {
            Function = function;
            SlotStart = slotStart;
            BaseSlot = baseSlot;
            InstructionPointer = 0;
        }

        public Instruction ReadInstruction()
        {
            return Function.Chunk.Instructions[InstructionPointer++];
        }
    }

    public sealed class VM
    {
        readonly Runtime runtime;
        readonly Dictionary<InternedString, RuntimeValue> globals = new();
        RuntimeValue[] stack = new RuntimeValue[256];
        int stackCount;
        CallFrame[] frames = new CallFrame[64];
        int frameCount;

        public VM(Runtime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public ExecutionResult Execute(RuntimeFunction function)
        {
            if (function == null)
            {
                return ExecutionResult.Fail("没有可执行的函数");
            }

            stackCount = 0;
            frameCount = 0;
            PushFrame(function, 0, 0);

            try
            {
                RuntimeValue value = Run();
                return ExecutionResult.Ok(value.ToObject());
            }
            catch (RuntimeException ex)
            {
                string lineInfo = ex.Line > 0 ? $"[line {ex.Line}] " : string.Empty;
                return ExecutionResult.Fail(lineInfo + ex.Message);
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail(ex.Message);
            }
            finally
            {
                ClearStack();
                ClearFrames();
            }
        }

        RuntimeValue Run()
        {
            while (frameCount > 0)
            {
                ref CallFrame frame = ref CurrentFrame;
                if (frame.InstructionPointer >= frame.Function.Chunk.Count)
                {
                    return ReturnFromFrame(RuntimeValue.Nil);
                }

                Instruction instruction = frame.ReadInstruction();
                switch (instruction.Code)
                {
                    case OpCode.Constant:
                        Push(RuntimeValue.FromObject(frame.Function.Chunk.Constants.Get((int)instruction.Operand)));
                        break;
                    case OpCode.Nil:
                        Push(RuntimeValue.Nil);
                        break;
                    case OpCode.True:
                        Push(RuntimeValue.FromBool(true));
                        break;
                    case OpCode.False:
                        Push(RuntimeValue.FromBool(false));
                        break;
                    case OpCode.Pop:
                        Pop(instruction.Line);
                        break;
                    case OpCode.DefineGlobal:
                        DefineGlobal((NameOperand)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.GetGlobal:
                        GetGlobal((NameOperand)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.SetGlobal:
                        SetGlobal((NameOperand)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.GetLocal:
                        Push(GetLocal((int)instruction.Operand, instruction.Line));
                        break;
                    case OpCode.SetLocal:
                        SetLocal((int)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.Add:
                        Add(instruction.Line);
                        break;
                    case OpCode.Subtract:
                        Subtract(instruction.Line);
                        break;
                    case OpCode.Multiply:
                        Multiply(instruction.Line);
                        break;
                    case OpCode.Divide:
                        Divide(instruction.Line);
                        break;
                    case OpCode.Negate:
                        Negate(instruction.Line);
                        break;
                    case OpCode.Not:
                        Not(instruction.Line);
                        break;
                    case OpCode.Equal:
                        Equal(instruction.Line);
                        break;
                    case OpCode.Greater:
                        Greater(instruction.Line);
                        break;
                    case OpCode.GreaterEqual:
                        GreaterEqual(instruction.Line);
                        break;
                    case OpCode.Less:
                        Less(instruction.Line);
                        break;
                    case OpCode.LessEqual:
                        LessEqual(instruction.Line);
                        break;
                    case OpCode.And:
                        LogicalAnd(instruction.Line);
                        break;
                    case OpCode.Or:
                        LogicalOr(instruction.Line);
                        break;
                    case OpCode.Jump:
                        frame.InstructionPointer = (int)instruction.Operand;
                        break;
                    case OpCode.JumpIfFalse:
                        if (!Peek(instruction.Line).IsTruthy())
                        {
                            frame.InstructionPointer = (int)instruction.Operand;
                        }
                        break;
                    case OpCode.Loop:
                        frame.InstructionPointer = (int)instruction.Operand;
                        break;
                    case OpCode.Call:
                        CallValue((int)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.Return:
                        RuntimeValue result = stackCount > 0 ? Pop(instruction.Line) : RuntimeValue.Nil;
                        RuntimeValue final = ReturnFromFrame(result);
                        if (frameCount == 0)
                        {
                            return final;
                        }
                        break;
                    case OpCode.InvokeCommand:
                        InvokeCommand((CommandOperand)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.GetExternal:
                        Push(RuntimeValue.FromObject(runtime.GetExternal(((NameOperand)instruction.Operand).Name)));
                        break;
                    case OpCode.SetExternal:
                        runtime.SetExternal(((NameOperand)instruction.Operand).Name, Peek(instruction.Line).ToObject());
                        break;
                    case OpCode.GetMember:
                        Push(RuntimeValue.FromObject(runtime.GetMember(Pop(instruction.Line).ToObject(), ((NameOperand)instruction.Operand).Name)));
                        break;
                    case OpCode.SetMember:
                        SetMember((NameOperand)instruction.Operand, instruction.Line);
                        break;
                    case OpCode.InvokeMember:
                        InvokeMember((MemberInvokeOperand)instruction.Operand, instruction.Line);
                        break;
                    default:
                        throw new RuntimeException($"未知字节码 {instruction.Code}", instruction.Line);
                }
            }

            return RuntimeValue.Nil;
        }

        ref CallFrame CurrentFrame => ref frames[frameCount - 1];

        void DefineGlobal(NameOperand operand, int line)
        {
            RuntimeValue value = Pop(line);
            globals[operand.Name] = value;
        }

        void GetGlobal(NameOperand operand, int line)
        {
            if (globals.TryGetValue(operand.Name, out RuntimeValue value))
            {
                Push(value);
                return;
            }

            if (runtime.HasCommand(operand.Name))
            {
                Push(RuntimeValue.FromCommand(operand.Name));
                return;
            }

            throw new RuntimeException($"变量未定义 {operand.Name.Value}", line);
        }

        void SetGlobal(NameOperand operand, int line)
        {
            if (!globals.ContainsKey(operand.Name))
            {
                throw new RuntimeException($"变量未定义 {operand.Name.Value}", line);
            }

            globals[operand.Name] = Peek(line);
        }

        RuntimeValue GetLocal(int slot, int line)
        {
            int index = CurrentFrame.SlotStart + slot;
            if (index < 0 || index >= stackCount)
            {
                throw new RuntimeException($"局部变量槽位无效 {slot}", line);
            }

            return stack[index];
        }

        void SetLocal(int slot, int line)
        {
            int index = CurrentFrame.SlotStart + slot;
            if (index < 0 || index >= stackCount)
            {
                throw new RuntimeException($"局部变量槽位无效 {slot}", line);
            }

            stack[index] = Peek(line);
        }

        void Add(int line)
        {
            RequireStackCount(2, line);
            int leftIndex = stackCount - 2;
            RuntimeValue left = stack[leftIndex];
            RuntimeValue right = stack[leftIndex + 1];
            if (left.Kind == RuntimeValueKind.Number && right.Kind == RuntimeValueKind.Number)
            {
                stack[leftIndex] = RuntimeValue.FromNumber(left.Number + right.Number);
                stackCount--;
                return;
            }

            if (left.Kind == RuntimeValueKind.String || right.Kind == RuntimeValueKind.String)
            {
                stack[leftIndex] = RuntimeValue.FromString(left.ToDisplayString() + right.ToDisplayString());
                stackCount--;
                return;
            }

            throw new RuntimeException("+ 只能用于数字相加或字符串拼接", line);
        }

        void Subtract(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromNumber(left.Number - right.Number);
            stackCount--;
        }

        void Multiply(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromNumber(left.Number * right.Number);
            stackCount--;
        }

        void Divide(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromNumber(left.Number / right.Number);
            stackCount--;
        }

        void Negate(int line)
        {
            RequireStackCount(1, line);
            int index = stackCount - 1;
            RuntimeValue value = stack[index];
            if (value.Kind != RuntimeValueKind.Number)
            {
                throw new RuntimeException("操作数必须是数字", line);
            }

            stack[index] = RuntimeValue.FromNumber(-value.Number);
        }

        void Not(int line)
        {
            RequireStackCount(1, line);
            int index = stackCount - 1;
            stack[index] = RuntimeValue.FromBool(!stack[index].IsTruthy());
        }

        void Equal(int line)
        {
            RequireStackCount(2, line);
            int leftIndex = stackCount - 2;
            stack[leftIndex] = RuntimeValue.FromBool(stack[leftIndex].EqualsValue(stack[leftIndex + 1]));
            stackCount--;
        }

        void Greater(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromBool(left.Number > right.Number);
            stackCount--;
        }

        void GreaterEqual(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromBool(left.Number >= right.Number);
            stackCount--;
        }

        void Less(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromBool(left.Number < right.Number);
            stackCount--;
        }

        void LessEqual(int line)
        {
            RequireNumberPair(line, out int leftIndex, out RuntimeValue left, out RuntimeValue right);
            stack[leftIndex] = RuntimeValue.FromBool(left.Number <= right.Number);
            stackCount--;
        }

        void LogicalAnd(int line)
        {
            RequireStackCount(2, line);
            int leftIndex = stackCount - 2;
            stack[leftIndex] = RuntimeValue.FromBool(stack[leftIndex].IsTruthy() && stack[leftIndex + 1].IsTruthy());
            stackCount--;
        }

        void LogicalOr(int line)
        {
            RequireStackCount(2, line);
            int leftIndex = stackCount - 2;
            stack[leftIndex] = RuntimeValue.FromBool(stack[leftIndex].IsTruthy() || stack[leftIndex + 1].IsTruthy());
            stackCount--;
        }

        void CallValue(int argumentCount, int line)
        {
            int calleeIndex = stackCount - argumentCount - 1;
            if (calleeIndex < 0)
            {
                throw new RuntimeException("调用参数数量异常", line);
            }

            RuntimeValue callee = stack[calleeIndex];
            if (callee.Kind == RuntimeValueKind.Function)
            {
                RuntimeFunction function = callee.Function;
                if (argumentCount != function.Arity)
                {
                    throw new RuntimeException($"{function.Name} 需要 {function.Arity} 个参数，实际传入 {argumentCount} 个", line);
                }

                PushFrame(function, calleeIndex + 1, calleeIndex);
                return;
            }

            if (callee.Kind == RuntimeValueKind.Command)
            {
                List<object> args = ReadArguments(calleeIndex + 1, argumentCount);
                RemoveStackRange(calleeIndex, argumentCount + 1);
                Push(RuntimeValue.FromObject(runtime.InvokeCommand(callee.CommandName, args)));
                return;
            }

            throw new RuntimeException("该值不可调用", line);
        }

        void InvokeCommand(CommandOperand operand, int line)
        {
            List<object> args = PopArguments(operand.ArgumentCount, line);
            Push(RuntimeValue.FromObject(runtime.InvokeCommand(operand.Name, args)));
        }

        void InvokeMember(MemberInvokeOperand operand, int line)
        {
            List<object> args = PopArguments(operand.ArgumentCount, line);
            object target = Pop(line).ToObject();
            Push(RuntimeValue.FromObject(runtime.InvokeMember(target, operand.Name, args)));
        }

        void SetMember(NameOperand operand, int line)
        {
            RuntimeValue value = Pop(line);
            object target = Pop(line).ToObject();
            runtime.SetMember(target, operand.Name, value.ToObject());
            Push(value);
        }

        RuntimeValue ReturnFromFrame(RuntimeValue result)
        {
            CallFrame frame = CurrentFrame;
            int removeCount = stackCount - frame.BaseSlot;
            if (removeCount > 0)
            {
                RemoveStackRange(frame.BaseSlot, removeCount);
            }

            frameCount--;
            frames[frameCount] = default;
            if (frameCount > 0)
            {
                Push(result);
            }

            return result;
        }

        void PushFrame(RuntimeFunction function, int slotStart, int baseSlot)
        {
            if (frameCount == frames.Length)
            {
                Array.Resize(ref frames, frames.Length * 2);
            }

            frames[frameCount].Reset(function, slotStart, baseSlot);
            frameCount++;
        }

        void ClearFrames()
        {
            for (int i = 0; i < frameCount; i++)
            {
                frames[i] = default;
            }

            frameCount = 0;
        }

        List<object> PopArguments(int count, int line)
        {
            if (count < 0 || count > stackCount)
            {
                throw new RuntimeException("参数栈数量异常", line);
            }

            int start = stackCount - count;
            List<object> args = ReadArguments(start, count);
            RemoveStackRange(start, count);
            return args;
        }

        List<object> ReadArguments(int start, int count)
        {
            List<object> args = new(count);
            for (int i = 0; i < count; i++)
            {
                args.Add(stack[start + i].ToObject());
            }

            return args;
        }

        void Push(RuntimeValue value)
        {
            if (stackCount == stack.Length)
            {
                Array.Resize(ref stack, stack.Length * 2);
            }

            stack[stackCount++] = value;
        }

        RuntimeValue Pop(int line)
        {
            if (stackCount == 0)
            {
                throw new RuntimeException("运行栈为空", line);
            }

            stackCount--;
            RuntimeValue value = stack[stackCount];
            return value;
        }

        RuntimeValue Peek(int line)
        {
            if (stackCount == 0)
            {
                throw new RuntimeException("运行栈为空", line);
            }

            return stack[stackCount - 1];
        }

        void RemoveStackRange(int start, int count)
        {
            if (count <= 0)
                return;

            int end = start + count;
            int moveCount = stackCount - end;
            if (moveCount > 0)
            {
                Array.Copy(stack, end, stack, start, moveCount);
            }

            stackCount -= count;
        }

        void ClearStack()
        {
            Array.Clear(stack, 0, stack.Length);
            stackCount = 0;
        }

        static double RequireNumber(RuntimeValue value, int line)
        {
            if (value.Kind == RuntimeValueKind.Number)
            {
                return value.Number;
            }

            throw new RuntimeException("操作数必须是数字", line);
        }

        void RequireNumberPair(int line, out int leftIndex, out RuntimeValue left, out RuntimeValue right)
        {
            RequireStackCount(2, line);
            leftIndex = stackCount - 2;
            left = stack[leftIndex];
            right = stack[leftIndex + 1];
            if (left.Kind != RuntimeValueKind.Number || right.Kind != RuntimeValueKind.Number)
            {
                throw new RuntimeException("操作数必须是数字", line);
            }
        }

        void RequireStackCount(int count, int line)
        {
            if (stackCount < count)
            {
                throw new RuntimeException("运行栈为空", line);
            }
        }
    }
}

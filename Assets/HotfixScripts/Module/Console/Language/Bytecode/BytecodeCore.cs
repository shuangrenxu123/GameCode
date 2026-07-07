using System;
using System.Collections.Generic;

namespace Helper
{
    public sealed class InternedString : IEquatable<InternedString>
    {
        public string Value { get; }
        public int Hash { get; }

        internal InternedString(string value)
        {
            Value = value ?? string.Empty;
            Hash = StringComparer.Ordinal.GetHashCode(Value);
        }

        public bool Equals(InternedString other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;
            return Hash == other.Hash && string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is InternedString other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Hash;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public sealed class StringTable
    {
        readonly Dictionary<string, InternedString> strings = new(StringComparer.Ordinal);

        public InternedString Intern(string value)
        {
            value ??= string.Empty;
            if (strings.TryGetValue(value, out InternedString interned))
            {
                return interned;
            }

            interned = new InternedString(value);
            strings.Add(value, interned);
            return interned;
        }
    }

    public enum OpCode
    {
        Constant,
        Nil,
        True,
        False,
        Pop,
        DefineGlobal,
        GetGlobal,
        SetGlobal,
        GetLocal,
        SetLocal,
        Add,
        Subtract,
        Multiply,
        Divide,
        Negate,
        Not,
        Equal,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        And,
        Or,
        Jump,
        JumpIfFalse,
        Loop,
        Call,
        Return,
        InvokeCommand,
        GetExternal,
        SetExternal,
        GetMember,
        SetMember,
        InvokeMember
    }

    public readonly struct Instruction
    {
        public readonly OpCode Code;
        public readonly object Operand;
        public readonly int Line;

        public Instruction(OpCode code, object operand, int line)
        {
            Code = code;
            Operand = operand;
            Line = line;
        }
    }

    public readonly struct NameOperand
    {
        public readonly InternedString Name;

        public NameOperand(InternedString name)
        {
            Name = name;
        }
    }

    public readonly struct CommandOperand
    {
        public readonly InternedString Name;
        public readonly int ArgumentCount;

        public CommandOperand(InternedString name, int argumentCount)
        {
            Name = name;
            ArgumentCount = argumentCount;
        }
    }

    public readonly struct MemberInvokeOperand
    {
        public readonly InternedString Name;
        public readonly int ArgumentCount;

        public MemberInvokeOperand(InternedString name, int argumentCount)
        {
            Name = name;
            ArgumentCount = argumentCount;
        }
    }

    public sealed class ConstantPool
    {
        readonly List<object> constants = new();
        readonly Dictionary<object, int> knownConstants = new();

        public IReadOnlyList<object> Values => constants;

        public int Add(object value)
        {
            if (value != null && IsReusableConstant(value) && knownConstants.TryGetValue(value, out int index))
            {
                return index;
            }

            constants.Add(value);
            int newIndex = constants.Count - 1;
            if (value != null && IsReusableConstant(value))
            {
                knownConstants[value] = newIndex;
            }

            return newIndex;
        }

        public object Get(int index)
        {
            if (index < 0 || index >= constants.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return constants[index];
        }

        static bool IsReusableConstant(object value)
        {
            return value is string
                || value is double
                || value is bool
                || value is InternedString;
        }
    }

    public sealed class Chunk
    {
        readonly List<Instruction> instructions = new();

        public ConstantPool Constants { get; } = new();
        public IReadOnlyList<Instruction> Instructions => instructions;
        public int Count => instructions.Count;

        public int AddConstant(object value)
        {
            return Constants.Add(value);
        }

        public int Write(OpCode code, int line)
        {
            return Write(code, null, line);
        }

        public int Write(OpCode code, object operand, int line)
        {
            instructions.Add(new Instruction(code, operand, line));
            return instructions.Count - 1;
        }

        public void PatchOperand(int instructionIndex, object operand)
        {
            if (instructionIndex < 0 || instructionIndex >= instructions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(instructionIndex));
            }

            Instruction old = instructions[instructionIndex];
            instructions[instructionIndex] = new Instruction(old.Code, operand, old.Line);
        }
    }
}

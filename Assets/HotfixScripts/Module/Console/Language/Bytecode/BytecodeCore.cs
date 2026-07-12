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

    public enum OpCode : byte
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

    public sealed class ConstantPool
    {
        readonly List<RuntimeValue> constants = new();
        readonly Dictionary<RuntimeValue, int> knownConstants = new();

        public IReadOnlyList<RuntimeValue> Values => constants;

        public int Add(RuntimeValue value)
        {
            if (knownConstants.TryGetValue(value, out int index))
            {
                return index;
            }

            constants.Add(value);
            int newIndex = constants.Count - 1;
            knownConstants[value] = newIndex;

            return newIndex;
        }

        public RuntimeValue Get(int index)
        {
            if (index < 0 || index >= constants.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return constants[index];
        }
    }

    public sealed class Chunk
    {
        readonly List<byte> code = new();
        readonly List<int> lines = new();

        public ConstantPool Constants { get; } = new();
        public int Count => code.Count;

        public int AddConstant(RuntimeValue value)
        {
            return Constants.Add(value);
        }

        public int Write(OpCode code, int line)
        {
            return WriteByte((byte)code, line);
        }

        public int WriteByte(byte value, int line)
        {
            this.code.Add(value);
            lines.Add(line);
            return this.code.Count - 1;
        }

        public int WriteInt(int value, int line)
        {
            int offset = code.Count;
            WriteByte((byte)(value & 0xFF), line);
            WriteByte((byte)((value >> 8) & 0xFF), line);
            WriteByte((byte)((value >> 16) & 0xFF), line);
            WriteByte((byte)((value >> 24) & 0xFF), line);
            return offset;
        }

        public void PatchInt(int offset, int value)
        {
            if (offset < 0 || offset + 3 >= code.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            code[offset] = (byte)(value & 0xFF);
            code[offset + 1] = (byte)((value >> 8) & 0xFF);
            code[offset + 2] = (byte)((value >> 16) & 0xFF);
            code[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        public OpCode ReadOpCode(ref int offset)
        {
            return (OpCode)ReadByte(ref offset);
        }

        public byte ReadByte(ref int offset)
        {
            if (offset < 0 || offset >= code.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return code[offset++];
        }

        public int ReadInt(ref int offset)
        {
            if (offset < 0 || offset + 3 >= code.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            int value = code[offset]
                | (code[offset + 1] << 8)
                | (code[offset + 2] << 16)
                | (code[offset + 3] << 24);
            offset += 4;
            return value;
        }

        public int GetLine(int offset)
        {
            if (offset < 0 || offset >= lines.Count)
            {
                return 0;
            }

            return lines[offset];
        }
    }
}

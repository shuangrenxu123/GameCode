using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum CharacterInputMask
{
    None = 0,
    Jump = 1 << 0,
    Run = 1 << 1,
    Interact = 1 << 2,
    Roll = 1 << 3,
    Lock = 1 << 4,
    Attack = 1 << 5,
    HeavyAttack = 1 << 6,
    Crouch = 1 << 7,
    OpenUI = 1 << 8,
    OpenConsole = 1 << 9,
    Movement = 1 << 10,
    UIConfirm = 1 << 11,
    UICancel = 1 << 12,
    All = (1 << 13) - 1
}

public enum CharacterInputType
{
    Jump,
    Run,
    Interact,
    Roll,
    Lock,
    Attack,
    HeavyAttack,
    Crouch,
    OpenUI,
    OpenConsole,
    Movement,
    UIConfirm,
    UICancel
}

public enum CharacterInputPhase
{
    Started,
    Performed,
    Canceled
}

public readonly struct CharacterInputCommand
{
    public CharacterInputCommand(
        CharacterInputType type,
        CharacterInputPhase phase,
        bool boolValue,
        Vector2 vector2Value,
        float createTime,
        float expireTime,
        uint sequence)
    {
        Type = type;
        Phase = phase;
        BoolValue = boolValue;
        Vector2Value = vector2Value;
        CreateTime = createTime;
        ExpireTime = expireTime;
        Sequence = sequence;
    }

    public CharacterInputType Type { get; }
    public CharacterInputPhase Phase { get; }
    public bool BoolValue { get; }
    public Vector2 Vector2Value { get; }
    public float CreateTime { get; }
    public float ExpireTime { get; }
    public uint Sequence { get; }
}

public sealed class CharacterInputCommandQueue
{
    readonly List<CharacterInputCommand> commands;
    readonly int capacity;

    public CharacterInputCommandQueue(int capacity)
    {
        this.capacity = Mathf.Max(1, capacity);
        commands = new List<CharacterInputCommand>(this.capacity);
    }

    public int Count => commands.Count;

    public void Enqueue(in CharacterInputCommand command)
    {
        if (commands.Count >= capacity)
        {
            commands.RemoveAt(0);
        }

        commands.Add(command);
    }

    public bool TryConsume(CharacterInputType type, float currentTime, out CharacterInputCommand command)
    {
        RemoveExpired(currentTime);

        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].Type != type)
            {
                continue;
            }

            command = commands[i];
            commands.RemoveAt(i);
            return true;
        }

        command = default;
        return false;
    }

    public bool TryConsumeLatest(CharacterInputType type, float currentTime, out CharacterInputCommand command)
    {
        RemoveExpired(currentTime);

        int latestIndex = -1;
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            if (commands[i].Type == type)
            {
                latestIndex = i;
                break;
            }
        }

        if (latestIndex < 0)
        {
            command = default;
            return false;
        }

        command = commands[latestIndex];
        for (int i = latestIndex; i >= 0; i--)
        {
            if (commands[i].Type == type)
            {
                commands.RemoveAt(i);
            }
        }

        return true;
    }

    public void Remove(CharacterInputType type)
    {
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            if (commands[i].Type == type)
            {
                commands.RemoveAt(i);
            }
        }
    }

    public void RemoveExpired(float currentTime)
    {
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            if (commands[i].ExpireTime <= currentTime)
            {
                commands.RemoveAt(i);
            }
        }
    }

    public void Clear()
    {
        commands.Clear();
    }
}

public static class CharacterInputTypeExtensions
{
    public static CharacterInputMask ToMask(this CharacterInputType type)
    {
        return (CharacterInputMask)(1 << (int)type);
    }
}

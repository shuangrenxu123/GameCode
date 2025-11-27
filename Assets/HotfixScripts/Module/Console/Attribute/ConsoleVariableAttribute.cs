using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ConsoleVariableAttribute : Attribute
{
    public string Name { get; }

    public ConsoleVariableAttribute(string name = null)
    {
        Name = name;
    }
}

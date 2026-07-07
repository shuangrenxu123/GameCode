using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class VariableAttribute : Attribute
{
    public string Name { get; }

    public VariableAttribute(string name = null)
    {
        Name = name;
    }
}

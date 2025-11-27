using System;

public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public CommandAttribute(string name, string description = null)
    {
        Name = name;
        Description = description;
    }
}

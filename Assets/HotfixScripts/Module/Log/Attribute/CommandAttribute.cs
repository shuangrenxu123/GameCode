using System;

public class CommandAttribute : Attribute
{
    public string Name;
    public CommandAttribute(string name)
    {
        this.Name = name;
    }
}

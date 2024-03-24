using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntNumber : Number
{
    public IntNumber(int v,PropertySourceType type)
    {
        Value = v;
        this.Type = type;
    }
}
public enum PropertySourceType
{
    Equipe,
    Buff,
    Self,//¡Ÿ ± ˝÷µ
}

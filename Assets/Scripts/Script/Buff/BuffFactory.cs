using Fight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffFactory
{
    public static BuffBase CreateBuff(string name,CombatEntity c,BuffManager manager)
    {
        BuffBase buff = null;
        if(name == "poisoned")
            buff = CreatePoisoned(c, manager);
        if (name == "hemophagia")
            buff = CreateHemophagia(c,manager);
        return buff;
    }
    private static BuffBase CreatePoisoned(CombatEntity c,BuffManager manager)
    {
        BuffBase poisoned = new PoisonedBuff(manager,c);
        return poisoned;
    }
    private static BuffBase CreateHemophagia(CombatEntity c,BuffManager manager)
    {
        BuffBase hemophagia = new HemophagiaBuff(manager,c);
        return hemophagia;
    }
}

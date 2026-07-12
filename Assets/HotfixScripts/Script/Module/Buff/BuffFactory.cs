using Fight;

public enum BuffId
{
    None = 0,
    Poisoned = 1,
    Hemophagia = 2,
}

public class BuffFactory
{
    public static bool TryGetBuffName(BuffId buffId, out string buffName)
    {
        switch (buffId)
        {
            case BuffId.Poisoned:
                buffName = "poisoned";
                return true;
            case BuffId.Hemophagia:
                buffName = "hemophagia";
                return true;
            default:
                buffName = null;
                return false;
        }
    }

    public static BuffBase CreateBuff(string name, CombatEntity c, BuffManager manager)
    {
        BuffBase buff = null;
        if (name == "poisoned")
            buff = CreatePoisoned(c, manager);
        if (name == "hemophagia")
            buff = CreateHemophagia(c, manager);
        return buff;
    }
    private static BuffBase CreatePoisoned(CombatEntity c, BuffManager manager)
    {
        BuffBase poisoned = new PoisonedBuff(manager, c);
        return poisoned;
    }
    private static BuffBase CreateHemophagia(CombatEntity c, BuffManager manager)
    {
        BuffBase hemophagia = new HemophagiaBuff(manager, c);
        return hemophagia;
    }
}

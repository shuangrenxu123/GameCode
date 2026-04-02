namespace Fight.Number
{
    public enum PropertyType
    {
        Attack = 0,
        Defense = 1,
        SpeedMultiplier = 2,
        RotationMultiplier = 3,
        MaxHp = 4,
        AllRound = 5,
        DamageReductionRate = 6,
    }

    public enum ResourceType
    {
        Hp = 0,
    }

    public enum ModifierType
    {
        Add = 0,
        AddPercent = 1,
        Multiply = 2,
        Override = 3,
    }

    public enum ModifierSourceType
    {
        None = 0,
        Self = 1,
        Equipment = 2,
        Buff = 3,
        Skill = 4,
        Talent = 5,
    }
}

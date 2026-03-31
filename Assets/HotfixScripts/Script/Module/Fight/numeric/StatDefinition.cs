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

    public readonly struct StatDefinition
    {
        public readonly int DefaultBaseValue;
        public readonly int MinFinalValue;
        public readonly int MaxFinalValue;

        public StatDefinition(int defaultBaseValue, int minFinalValue, int maxFinalValue)
        {
            DefaultBaseValue = defaultBaseValue;
            MinFinalValue = minFinalValue;
            MaxFinalValue = maxFinalValue;
        }
    }
}

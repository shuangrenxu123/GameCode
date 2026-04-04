using Sirenix.OdinInspector;

namespace Fight.Number
{
    public enum PropertyType
    {
        [LabelText("攻击")]
        Attack = 0,

        [LabelText("防御")]
        Defense = 1,

        [LabelText("移速倍率")]
        SpeedMultiplier = 2,

        [LabelText("转向倍率")]
        RotationMultiplier = 3,

        [LabelText("最大生命")]
        MaxHp = 4,

        [LabelText("全能")]
        AllRound = 5,

        [LabelText("减伤率")]
        DamageReductionRate = 6,
    }

    public enum ResourceType
    {
        [LabelText("生命")]
        Hp = 0,

        [LabelText("护盾")]
        Shield = 1,
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

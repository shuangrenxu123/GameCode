/// <summary>
/// 技能类型
/// </summary>
public enum SkillType
{
    /// <summary>
    /// 主动技能
    /// </summary>
    Initiative,
    /// <summary>
    /// 被动技能
    /// </summary>
    Passive,
}
/// <summary>
/// 目标选择类型
/// </summary>
public enum SkillTargetSelectType
{
    /// <summary>
    /// 手动选择
    /// </summary>
    playerSelect,
    /// <summary>
    /// 区域选择
    /// </summary>
    CollisionSelect,
    /// <summary>
    /// 条件选择
    /// </summary>
    conditionSelect,
}
/// <summary>
/// 技能作用对象
/// </summary>
public enum SkillAffectTargetType
{
    Self,
    SelfTeam,
    EnemyTeam,
}
/// <summary>
/// 技能作用数量
/// </summary>
public enum SkillTargetType
{
    /// <summary>
    /// 单体
    /// </summary>
    Single,

    /// <summary>
    /// 群体
    /// </summary>
    Multiple,
}


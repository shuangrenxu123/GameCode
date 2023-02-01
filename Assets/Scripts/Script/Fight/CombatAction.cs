/// <summary>
/// һ����Ϊ
/// </summary>
public abstract class CombatAction
{
    /// <summary>
    /// ������
    /// </summary>
    public CombatEntity Creator { get; set; }
    /// <summary>
    /// ����
    /// </summary>
    public CombatEntity Target { get; set; }
    /// <summary>
    /// ǰ��
    /// </summary>
    public abstract void PreProcess();
    /// <summary>
    /// Ӧ��
    /// </summary>
    public abstract void Allpy();
    /// <summary>
    /// ����
    /// </summary>
    public abstract void PostProcess();

}

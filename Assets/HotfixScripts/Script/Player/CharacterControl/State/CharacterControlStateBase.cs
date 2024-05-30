using HFSM;

public class CharacterControlStateBase : StateBase
{
    public CharacterActor CharacterActor { get; protected set; }
    public AnimactorHelper Animancer { get; set; }
    protected CharacterBrain CharacterBrain = null;
    public CharacterActions CharacterActions
    {
        get
        {
            return CharacterBrain == null ?
                new CharacterActions() : CharacterBrain.CharacterActions;
        }
    }
    public CharacterStateController_New CharacterStateController { get; protected set; }
    protected CCAnimatorConfig animatorConfig => CharacterStateController.stateManger.animatorConfig;
    protected NetTranform netHelper => CharacterStateController.stateManger.NetHelper;
    //public Dictionary<string, ClipTransition> animators;
    public override void Init()
    {
        CharacterActor = (parentMachine as CharacterStateController_New).CharacterActor;
        CharacterBrain = (parentMachine as CharacterStateController_New).CharacterBrain;
        CharacterStateController = (parentMachine as CharacterStateController_New);
    }
    public virtual void UpdateIK(int layerIndex)
    {

    }
    /// <summary>
    /// 在主循环结束后执行
    /// </summary>
    public virtual void PostUpdate()
    {
    }
    /// <summary>
    /// 该方法会在主循环之前执行
    /// </summary>
    public virtual void PreUpdate()
    {
    }

    /// <summary>
    /// 该方法会在物理模拟前执行
    /// </summary>
    public virtual void PreCharacterSimulation()
    {
    }

    /// <summary>
    /// 此方法在角色物理模拟之后运行。
    /// </summary>
    public virtual void PostCharacterSimulation()
    {
    }
    //public void AddStateAnimators(List<ClipTransition> anims)
    //{
    //    if (anims == null || anims.Count == 0)
    //    {
    //        Debug.LogError("添加动画失败" + name);
    //        return;
    //    }
    //    if (animators == null)
    //        animators = new Dictionary<string, ClipTransition>(anims.Count);
    //    for (int i = 0; i < anims.Count; i++)
    //    {
    //        animators.Add(anims[i].Clip.name, anims[i]);
    //    }
    //}
}

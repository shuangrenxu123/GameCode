using HFSM;
using UnityEngine;

public class CharacterControlStateBase : StateBase
{
    bool overrideAnimatorController = true;

    public CharacterActor CharacterActor { get; protected set; }


    /// <summary>
    /// 获得动画控制器
    /// </summary>
    public RuntimeAnimatorController RuntimeAnimatorController { get; set; }
    public bool OverrideAnimatorController => overrideAnimatorController;
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

}

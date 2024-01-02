using UnityEngine;

public class AttackState : CharacterControlStateBase
{
    private CharacterControlStateBase lastState;
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        lastState = (CharacterControlStateBase)CharacterStateController.lastState;
        if (RuntimeAnimatorController != null)
        {
            var animator = new AnimatorOverrideController(RuntimeAnimatorController);
            CharacterStateController.Animator.runtimeAnimatorController = animator;
        }
        var movestate = (lastState as MovementState);
        if ( movestate!= null)
        {
            if (movestate.isCrouched)
            {
                Debug.Log(2222);
            }
            Debug.Log("111");
        }
    }
    public override void Update()
    {
        AnimatorStateInfo info = CharacterStateController.Animator.GetCurrentAnimatorStateInfo(0);

        if (info.normalizedTime >= 1.0f)
        {
            database.SetData<bool>("attack", false);
        }
    }

}

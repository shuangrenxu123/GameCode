using UnityEngine;

public class InteractionState : CharacterControlStateBase
{
    #region Animator
    public string type = "type";
    #endregion
    private Interactable interactable;
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        interactable = database.GetData<Interactable>("interactable");
        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        if (RuntimeAnimatorController != null)
        {
            var animator = new AnimatorOverrideController(RuntimeAnimatorController);
            CharacterStateController.Animator.runtimeAnimatorController = animator;
        }
        CharacterActor.Animator.SetFloat("type",(int)interactable.InteractableType);
    }
    public override void Update()
    {
        AnimatorStateInfo info = CharacterStateController.Animator.GetCurrentAnimatorStateInfo(0);

        if (info.normalizedTime >= 1.0f)
        {
            database.SetData<bool>("interaction", false);
        }
    }
}
public enum InteractableType
{
    Door_Double = 1,
    Door_Up = 2,
    Door_Outward = 3,
    Chest=4,
    Lever=5,
}

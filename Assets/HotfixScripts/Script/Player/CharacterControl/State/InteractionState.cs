using Animancer;
using UnityEngine;

public class InteractionState : CharacterControlStateBase
{
    private Interactable interactable;
    private AnimancerState state;
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        interactable = database.GetData<Interactable>("interactable");
        if (interactable == null)
        {
            database.SetData<bool>("interaction", false);
        }
        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);


        state = Animancer.Play(animators[interactable.InteractableType.ToString()]);


        state.Events.OnEnd += OnAnimatorEnd;
        state.Events.Add(0.6f, OnInteract);
        interactable.StartInteract(CharacterStateController.stateManger.player);
    }
    private void OnAnimatorEnd()
    {
        database.SetData<bool>("interaction", false);
        interactable.EndInteract(CharacterStateController.stateManger.player);
        state.Events.Clear();
    }
    public void OnInteract()
    {
        interactable.Interact(CharacterStateController.stateManger.player);
    }
}
public enum InteractableType
{
    Door_Double = 1,
    Door_Up = 2,
    Door_Outward = 3,
    Chest = 4,
    Lever = 5,
    Item = 6
}

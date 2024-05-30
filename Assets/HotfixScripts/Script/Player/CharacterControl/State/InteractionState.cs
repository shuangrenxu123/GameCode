using Animancer;
using Fight;
using UnityEngine;

public class InteractionState : CharacterControlStateBase
{
    private Interactable interactable;
    private ConsumableItemData ItemData;
    private AnimancerState state;
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        interactable = database.GetData<Interactable>("interactable");
        if (interactable != null)
        {
            CharacterActor.Velocity = Vector3.zero;
            CharacterActor.SetUpRootMotion(true, RootMotionVelocityType.SetVelocity, false);

            state = Animancer.Play(animatorConfig.clipAnimators[interactable.InteractableType.ToString()]);

            state.Events.OnEnd += OnAnimatorEnd;
            state.Events.Add(0.6f, OnInteract);
            interactable.StartInteract(CharacterStateController.stateManger.player);
        }
        else
        {

            ItemData = database.GetData<ConsumableItemData>("interactionData");
            if (ItemData != null)
            {
                CharacterActor.SetUpRootMotion(true, RootMotionVelocityType.SetVelocity, false);
                state = Animancer.Play(animatorConfig.linearMixerAnimators[ItemData.consumeAnimation]);
                state.Events.Add(ItemData.effectTime, () => { ItemData.Effect(database.GetData<CombatEntity>("CombatEntity")); });
                state.Events.OnEnd += () =>
                {
                    database.SetData<bool>("interaction", false);
                    state.Events.Clear();
                };

            }
            else
            {
                database.SetData<bool>("interaction", false);
            }
        }
    }
    private void OnAnimatorEnd()
    {
        database.SetData<bool>("interaction", false);
        database.SetData<Interactable>("Interactable", null);
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

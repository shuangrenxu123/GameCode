using System;
using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LogicState
{
    enum AnimClipName
    {
        Roll,

    }
    public class CharacterEmptyLogicState : CharacterLogicBaseState
    {
        public override ECharacterLogicState currentType => ECharacterLogicState.Empty;

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter();
            combatEntity.hp.OnHit += TryChangeHitState;
        }

        public override void Exit()
        {
            base.Exit();
            combatEntity.hp.OnHit -= TryChangeHitState;
        }

        private void TryChangeHitState()
        {
            parentMachine.ChangeState(ECharacterLogicState.InjIry, new CharacterInjIryStateInput(0));
        }

        public override void Update()
        {
            base.Update();

            //进入与物体交互
            if (characterActor.Triggers.Count != 0)
            {
                var trigger = characterActor.Triggers[0];
                var Interaction = trigger.gameObject.GetComponent<Intractable>();
                if (Interaction && characterActions.interact.Started)
                {
                    var animClipName = GetInteractionAnimClipName(Interaction);

                    parentMachine.ChangeState(ECharacterLogicState.Interaction,
                        new InteractionState(animClipName,
                            true,
                            true,
                            Interaction.reference.position,
                            Interaction.reference.rotation,
                            Interaction.Interactive, null));

                    return;
                }
            }
            //进入翻滚
            if (characterActions.roll.Started &&
                movementMachine.CurrentStateType is ECharacterMoveState.NormalMove or ECharacterMoveState.RunMove)
            {
                parentMachine.ChangeState(ECharacterLogicState.Interaction,
                       new InteractionState(AnimClipName.Roll.ToString(),
                           false,
                           true,
                           default,
                           default,
                           null, null));
            }

            if (characterActions.attack.Started)
            {
                parentMachine.ChangeState(ECharacterLogicState.Attack);

                return;
            }
        }

        string GetInteractionAnimClipName(Intractable intractable)
        {
            return intractable.intractableType.ToString();
        }
    }
}

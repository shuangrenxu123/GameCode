using System;
using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LogicState
{
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
            if (characterActor.Triggers.Count != 0)
            {
                var trigger = characterActor.Triggers[0];
                var Interaction = trigger.gameObject.GetComponent<Intractable>();
                if (Interaction && characterActions.interact.Started)
                {
                    parentMachine.ChangeState(ECharacterLogicState.Interaction);
                    return;
                }
            }

            if (characterActions.attack.Started)
            {
                parentMachine.ChangeState(ECharacterLogicState.Attack);

                return;
            }
        }
    }
}

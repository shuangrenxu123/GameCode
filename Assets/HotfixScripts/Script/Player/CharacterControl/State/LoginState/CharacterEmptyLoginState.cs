using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LoginState
{
    public class CharacterEmptyLoginState : CharacterLogicBaseState
    {
        public override ECharacterLoginState currentType => ECharacterLoginState.Empty;

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter();
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
                    parentMachine.ChangeState(ECharacterLoginState.Interaction);
                    return;
                }
            }

            if (characterActions.attack.Started)
            {
                parentMachine.ChangeState(ECharacterLoginState.Attack);

                return;
            }
        }
    }
}

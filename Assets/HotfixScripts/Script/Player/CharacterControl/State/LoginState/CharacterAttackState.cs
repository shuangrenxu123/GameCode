using System;
using Character.Controller.State;
using Fight;
using HFSM;
using UnityEngine;

namespace Character.Controller.LoginState
{
    public class CharacterAttackState : CharacterLogicBaseState
    {
        public override ECharacterLoginState currentType => ECharacterLoginState.Attack;
        public SkillRunner timelineExecutor;

        public CharacterWeaponAnimator attackAnimator;
        public WeaponActionChangeGraph actionChangeGraph;

        SingleAttackActionAnimationConfig currentActionData;

        string currentActionName = string.Empty;
        bool canInput = true;
        bool isInput = false;

        float timer = 0;
        float nextActionInputTime = 0;
        public override void Enter(StateBaseInput input = null)
        {
            base.Enter();

            if (characterActions.attack.Started)
            {
                if (currentActionName == string.Empty)
                {
                    // 获取当前动作名称
                    currentActionName = actionChangeGraph.GetDefaultActionName(AttackKeyBoard.Light,
                        parentMachine.movementStateMachine.CurrentStateType);
                }
                else
                {
                    // 获取下一个动作名称
                    currentActionName = actionChangeGraph.GetNextActionName(
                        AttackKeyBoard.Light, currentActionName,
                        parentMachine.movementStateMachine.CurrentStateType);
                }
                if (currentActionName == null)
                {
                    parentMachine.ChangeState(ECharacterLoginState.Empty);
                }
                else
                {
                    PlayerAction();
                    characterActor.Velocity = Vector3.zero;
                    characterActor.SetUpRootMotion(true, true);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (timer < nextActionInputTime)
            {
                timer += Time.deltaTime;
            }
            else
            {
                canInput = true;
            }

            timelineExecutor.OnUpdate();
            if (characterActions.attack.Started)
            {
                if (canInput && isInput == false)
                {
                    currentActionName = actionChangeGraph
                        .GetNextActionName(AttackKeyBoard.Light, currentActionName,
                        parentMachine.movementStateMachine.CurrentStateType);

                    timelineExecutor.onFinish.AddListener(PlayerNextAttackAction);
                    isInput = true;
                    return;
                }
            }

            if (timelineExecutor.isFinish)
            {
                parentMachine.ChangeState(ECharacterLoginState.Empty);
            }
        }

        private void PlayerNextAttackAction()
        {
            isInput = false;
            PlayerAction();

            timelineExecutor.onFinish.RemoveListener(PlayerNextAttackAction);
        }

        void PlayerAction()
        {
            //temp Test
            currentActionData = attackAnimator.animators[WeaponType.Gloves]
                    .attackAnimations[currentActionName];
            timelineExecutor.LoadTimeLineAsset
                (currentActionData.timelineAsset);
            nextActionInputTime = currentActionData.inputTime;
            timer = 0;
            canInput = false;
        }

        public override void Exit()
        {
            base.Exit();
            currentActionName = string.Empty;
            characterActor.SetUpRootMotion(false, false);
            // characterActor.UseRootMotion = false;
            parentMachine.movementStateMachine.RefreshAnimator();
        }
    }
}

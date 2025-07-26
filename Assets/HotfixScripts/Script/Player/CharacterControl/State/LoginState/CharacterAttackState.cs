using System;
using Character.Controller.State;
using Fight;
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
        public override void Enter()
        {
            base.Enter();
            parentMachine.movementStateMachine.DisableMachine();

            if (characterActions.attack.Started)
            {
                if (currentActionName == string.Empty)
                {
                    // 获取当前动作名称
                    currentActionName = actionChangeGraph.defaultActionName;
                }
                else
                {
                    // 获取下一个动作名称
                    currentActionName = actionChangeGraph.GetNextActionName(
                        AttackKeyBoard.Light, currentActionName,
                        parentMachine.movementStateMachine.CurrentStateType);
                }

                UpdateActionData();
                timelineExecutor.LoadTimeLineAsset
                    (currentActionData.timelineAsset);

                // currentActionName = string.Empty;
            }

            characterActor.Velocity = Vector3.zero;
            characterActor.SetUpRootMotion(true, true);
        }

        public override void Update()
        {
            base.Update();
            timelineExecutor.OnUpdate();
            if (characterActions.attack.Started)
            {
                if (canInput)
                {
                    currentActionName = actionChangeGraph
                        .GetNextActionName(AttackKeyBoard.Light, currentActionName,
                        parentMachine.movementStateMachine.CurrentStateType);

                    timelineExecutor.onFinish.AddListener(PlayerNextAttackAction);
                    canInput = false;
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
            UpdateActionData();
            canInput = true;
            timelineExecutor.LoadTimeLineAsset
                (currentActionData.timelineAsset);
            timelineExecutor.onFinish.RemoveListener(PlayerNextAttackAction);
        }

        void UpdateActionData()
        {
            //temp Test
            currentActionData = attackAnimator.animators[WeaponType.Gloves]
                    .attackAnimations[currentActionName];
        }

        public override void Exit()
        {
            base.Exit();
            currentActionName = string.Empty;
            characterActor.SetUpRootMotion(false, false);
            parentMachine.movementStateMachine.EnableMachine();
        }

    }
}

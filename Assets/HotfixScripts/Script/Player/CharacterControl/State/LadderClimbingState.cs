using UnityEngine;
namespace CharacterControllerStateMachine
{
    public class LadderClimbingState : CharacterControlStateBase
    {
        protected bool userInteractAction = true;
        protected bool filterByAngle = true;
        protected float maxAngle = 70f;
        public float headMaxDistance = 1f;
        #region Animation
        protected string bottomDownParmeter = "BottomDown";
        protected string bottomUpParmeter = "BottonUmip";
        protected string topDownParameter = "TopDown";
        protected string topUpParameter = "TopUp";
        protected string upParameter = "Up";
        protected string downParameter = "Down";

        protected string entryStateName = "Entry";
        protected string exitStateName = "Exit";
        protected string idleStateName = "Idle";
        #endregion
        public enum LadderClimbState
        {
            Entering,
            Exiting,
            Idling,
            Climbing,
        }
        LadderClimbState state;
        Ladder currentLadder = null;
        Vector3 targetPosition = Vector3.zero;
        bool forceExit = false;
        AnimatorStateInfo animatorStateInfo;
        bool isBottom = false;

        public override StateType currentType => throw new System.NotImplementedException();

        public override void Init()
        {
            CharacterActor = (parentMachine as CharacterStateController_New).CharacterActor;
            CharacterBrain = (parentMachine as CharacterStateController_New).CharacterBrain;
            CharacterStateController = (parentMachine as CharacterStateController_New);
        }
        public override void Enter()
        {
            CharacterActor.Velocity = Vector3.zero;
            CharacterActor.IsKinematic = true;
            CharacterActor.alwaysNotGrounded = true;

            currentLadder = database.GetData<Ladder>("currentLadder");
            //targetPosition = isBottom?currentLadder.BottomReference.position : currentLadder.TopReference.position;
            //CharacterActor.SetYaw(currentLadder.FacingDirectionVector);
            //CharacterActor.Position = targetPosition;

            //Root Motion
            CharacterActor.SetUpRootMotion(true, RootMotionVelocityType.SetVelocity, false);
            CharacterActor.Animator.SetTrigger(isBottom ? bottomDownParmeter : topDownParameter);

            state = LadderClimbState.Entering;
        }
        public override void Exit()
        {
            CharacterActor.IsKinematic = false;
            CharacterActor.alwaysNotGrounded = false;
            //currentLadder = null;

            forceExit = false;

            CharacterStateController.ResetIKWeights();

            CharacterActor.Velocity = Vector3.zero;
            CharacterActor.ForceGrounded();
        }
        public override void FixUpdate()
        {
            CharacterActor.Animator.SetBool("up", false);
            CharacterActor.Animator.SetBool("down", false);
            if (CharacterActions.movement.Up)
            {
                CharacterActor.Animator.SetBool("up", true);
                CharacterActor.Animator.SetBool("down", false);
            }
            else if (CharacterActions.movement.Down)
            {
                CharacterActor.Animator.SetBool("up", false);
                CharacterActor.Animator.SetBool("down", true);
            }
            if (Physics.Raycast(CharacterActor.Top, CharacterActor.Up, headMaxDistance))
            {
                CharacterActor.Animator.SetBool("end", true);

            }
        }
    }
}
using Character.Controller.State;

namespace HFSM
{
    public interface ICharacterRandomMoveStateMachine
    {
        ECharacterRandomMoveState CurrentType { get; }

        void Start();
        void Update();
    }
}
namespace HFSM
{
    public record StateBaseInput;
    public interface IState
    {
        void Init();
        void Enter(StateBaseInput input = null);
        void Update();
        void Exit();
    }
}

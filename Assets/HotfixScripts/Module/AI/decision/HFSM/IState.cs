namespace HFSM
{
    public interface IState
    {
        void Init();
        void Enter();
        void Update();
        void Exit();
    }
}

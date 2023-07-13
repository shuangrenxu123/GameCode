namespace HFSM
{
    public interface IState
    {
        string name { get; set; }
        void Init();
        void Enter();
        void Update();
        void Exit();
    }
}

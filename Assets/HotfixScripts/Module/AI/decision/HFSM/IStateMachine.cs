namespace HFSM
{
    public interface IStateMachine
    {
        public void ChangeState(string name);
        StateBase CurrentState { get; set; }
        StateBase lastState { get; set; }

    }
}

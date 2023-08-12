namespace HFSM
{
    public interface IStateMachine
    {
        public void ChangeState(string name);
        StateBase activeState { get; set; }
        StateBase lastState { get; set; }

    }
}

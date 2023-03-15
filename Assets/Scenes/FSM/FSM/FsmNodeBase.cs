public class FsmNodeBase
{
    protected AiData aiData;
    protected FiniteStateMachine fsm { get; }
    public FsmNodeBase(FiniteStateMachine fsm, AiData data)
    {
        aiData = data;
        this.fsm = fsm;
    }
}
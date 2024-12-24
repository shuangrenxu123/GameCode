using System;

namespace HFSM
{
    public interface IStateMachine
    {
        IStateMachine ParentFsm { get; set; }
    }
}

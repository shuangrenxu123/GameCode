using System;

namespace HFSM
{
    public interface IStateMachine<C>
    {
        public C lastStateType { get; }
        public C CurrentStateType { get;  }

    }
}

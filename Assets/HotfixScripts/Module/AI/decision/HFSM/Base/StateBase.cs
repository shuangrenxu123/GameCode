using System.Collections.Generic;
namespace HFSM
{
    public class StateBase : IState
    {
        public string name { get; set; }
        /// <summary>
        /// 该状态所处的状态机
        /// </summary>
        public IStateMachine parentMachine;
        public List<StateTransition> transitions;
        public DataBase database;
        public virtual void Enter()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void AddTransition(StateTransition transition)
        {
            transitions ??= new List<StateTransition>();
            transitions.Add(transition);
        }

        /// <summary>
        /// 在state添加到StateMachine的时候调用
        /// </summary>
        public virtual void Init()
        {

        }
    }
}

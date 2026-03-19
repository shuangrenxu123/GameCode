using System;
using System.Collections.Generic;
using AIBlackboard;
namespace HFSM
{
    public abstract class StateBase<T> : IState where T : Enum
    {
        public abstract T currentType { get; }

        /// <summary>
        /// 该状态所处的状态机
        /// </summary>
        public IStateMachine<T> parentMachine;
        public List<StateTransition<T>> transitions;


        public Blackboard database;

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        public virtual void Enter(StateBaseInput input = null)
        {

        }
        /// <summary>
        /// 退出状态时调用
        /// </summary>
        public virtual void Exit()
        {

        }
        /// <summary>
        /// 处于状态时候，每帧调用
        /// </summary>
        public virtual void Update()
        {

        }
        public virtual void FixUpdate()
        {

        }
        /// <summary>
        /// 当状态被移除的时候调用
        /// </summary>
        public virtual void Remove()
        {

        }
        /// <summary>
        /// 在state添加到StateMachine的时候调用
        /// </summary>
        public virtual void Init()
        {

        }
        public void AddTransition(StateTransition<T> transition)
        {
            transitions ??= new List<StateTransition<T>>();
            transitions.Add(transition);
        }

    }
}

using System;
using System.Collections.Generic;
namespace HFSM
{
    public class StateBase<T> : IState where T : Enum
    {

        /// <summary>
        /// 该状态所处的状态机
        /// </summary>
        public IStateMachine parentMachine;
        public List<StateTransition<T>> transitions;


        public DataBase database;

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        public virtual void Enter()
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

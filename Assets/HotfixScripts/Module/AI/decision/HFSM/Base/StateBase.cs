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
        public int StateNameHash { get; private set; }
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
        /// 添加一组切换条件
        /// </summary>
        /// <param name="transition"></param>
        public virtual void AddTransition(StateTransition transition)
        {
            transitions ??= new List<StateTransition>();
            transitions.Add(transition);
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
    }
}

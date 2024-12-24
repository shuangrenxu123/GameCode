using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 有限状态机类
/// </summary>
namespace HFSM
{

    public class StateMachine<T> : StateMachine<T, T> where T : Enum
    {

    }
    public class StateMachine<T, C> : StateBase<T>, IStateMachine where T : Enum where C : Enum
    {
        public Dictionary<C, StateBase<C>> status = new();
        /// <summary>
        /// 当前状态的切换
        /// </summary>
        public List<StateTransition<C>> activeTransitions;
        /// <summary>
        /// 当前激活的状态
        /// </summary>
        public C CurrentState { get; set; }

        /// <summary>
        /// 上一个状态
        /// </summary>
        public C lastState { get; set; }

        /// <summary>
        /// 默认状态（即进入该状态机以后进入的子状态，默认为第一个添加的状态）
        /// </summary>
        public C defaultState { get; set; }

        /// <summary>
        /// 是否是根状态机
        /// </summary>
        public bool isRootMachine { get { return parentMachine == null; } }

        public IStateMachine ParentFsm { get; set; }

        /// <summary>
        /// 一个空过渡线,据说可以节省性能
        /// </summary>
        private static readonly List<StateTransition<C>> noTransitions = new(0);

        private bool isRunning = false;
        /// <summary>
        /// 运行有限状态机，不然不会调用Init与Enter函数
        /// </summary>
        public void Start()
        {
            if (!isRootMachine)
            {
                return;
            }

            Init();
            isRunning = true;
            Enter();
        }

        public void AddState(C nodeName, StateBase<C> state)
        {
            state.database = database;
            state.parentMachine = this;
            state.Init();
            if (status.Count == 0)
            {
                defaultState = nodeName;
            }
            if (status.ContainsKey(nodeName))
                Debug.LogError("状态名重复" + nodeName);
            else
                status.Add(nodeName, state);
        }

        /// <summary>
        /// 给当前状态机下某个状态添加一个Transitions
        /// </summary>
        /// <param name="transition"></param>
        public void AddTransition(StateTransition<C> transition)
        {
            if (status.TryGetValue(transition.startState, out StateBase<C> state))
            {
                state.AddTransition(transition);
            }
            else
            {
                Debug.LogError($"没有在状态机找到状态{transition.startState}");
            }
        }
        /// <summary>
        /// 检测能否切换为另一个状态，如果可以转化则直接切换
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        private bool CheckTransition(StateTransition<C> transition)
        {
            if (!transition.OnCheck())
            {
                return false;
            }
            ChangeState(transition.endState);
            return true;
        }
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="StateName"></param>
        public void ChangeState(C stateName)
        {
            var currState = FindState(CurrentState);
            currState?.Exit();
            lastState = CurrentState;

            var nextState = FindState(stateName);

            activeTransitions = nextState.transitions ?? noTransitions;

            CurrentState = stateName;
            nextState.Enter();
        }
        /// <summary>
        /// 修改当前状态机的默认状态
        /// </summary>
        /// <param name="stateType"></param>
        public void SetDefaultState(C stateType)
        {
            if (!status.TryGetValue(stateType, out StateBase<C> state))
            {
                Debug.LogError("不存在状态" + stateType);
            }
            else
            {
                defaultState = stateType;
            }
        }

        public virtual StateBase<C> FindState(C stateType)
        {
            status.TryGetValue(stateType, out StateBase<C> state);
            if (state == null)
            {
                throw new Exception($"Not Find State {stateType}");
            }
            return state;
        }


        #region  CallBack
        /// <summary>
        /// 需要在子类实现,该函数会在添加到状态机（即使状态机没有启动）时候调用
        /// </summary>
        public override void Init()
        {

        }
        public override void Enter()
        {
            base.Enter();

            if (defaultState == null)
            {
                return;
            }

            ChangeState(defaultState);
        }
        public override void Exit()
        {
            base.Exit();
            var state = FindState(CurrentState);

            state.Exit();
        }
        public override void Update()
        {
            var state = FindState(CurrentState);

            if (isRunning == false)
                return;

            foreach (var i in activeTransitions)
            {
                if (CheckTransition(i))
                {
                    break;
                }
            }
            state.Update();

        }
        public override void FixUpdate()
        {
            var state = FindState(CurrentState);

            if (isRunning == false)
                return;

            state.FixUpdate();
        }
        #endregion
    }
}
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 有限状态机类
/// </summary>
namespace HFSM
{
    public class StateMachine : StateBase, IStateMachine
    {
        public Dictionary<string, StateBase> status = new Dictionary<string, StateBase>();
        /// <summary>
        /// 当前状态的切换
        /// </summary>
        public List<StateTransition> activeTransitions;
        /// <summary>
        /// 当前激活的状态
        /// </summary>
        public StateBase CurrentState { get; set; }
        /// <summary>
        /// 上一个状态
        /// </summary>
        public StateBase lastState { get; set; }
        /// <summary>
        /// 默认状态（即进入该状态机以后进入的子状态，默认为第一个添加的状态）
        /// </summary>
        public StateBase defaultState { get; set; }
        /// <summary>
        /// 是否是根状态机
        /// </summary>
        public bool isRootMachine { get { return parentMachine == null; } }
        /// <summary>
        /// 一个空过渡线,据说可以节省性能
        /// </summary>
        private static readonly List<StateTransition> noTransitions = new(0);

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
        public void AddState(string nodeName, StateBase state)
        {
            state.database = database;
            state.name = nodeName;
            state.parentMachine = this;
            state.Init();
            if (status.Count == 0)
            {
                defaultState = state;
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
        public override void AddTransition(StateTransition transition)
        {
            if (status.TryGetValue(transition.startState, out StateBase state))
            {
                state.AddTransition(transition);
            }
            else
            {
                Debug.LogError($"没有在状态机{name}找到状态{transition.startState}");
            }
        }
        /// <summary>
        /// 检测能否切换为另一个状态，如果可以转化则直接切换
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        private bool CheckTransition(StateTransition transition)
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
        public void ChangeState(string stateName)
        {
            CurrentState?.Exit();
            lastState = CurrentState;
            if (!status.TryGetValue(stateName, out StateBase newState))
            {
                Debug.Log("没有该状态");
            }
            activeTransitions = newState.transitions ?? noTransitions;
            CurrentState = newState;
            CurrentState.Enter();
        }
        /// <summary>
        /// 修改当前状态机的默认状态
        /// </summary>
        /// <param name="stateName"></param>
        public void SetDefaultState(string stateName)
        {
            if (!status.TryGetValue(stateName, out StateBase state))
            {
                Debug.LogError("不存在状态" + stateName);
            }
            else
            {
                defaultState = state;
            }
        }
        /// <summary>
        /// 需要在子类实现,该函数会在添加到状态机（即使状态机没有启动）时候调用
        /// </summary>
        public override void Init()
        {
            Debug.Log("Init" + name);
        }
        public override void Enter()
        {
            base.Enter();
            Debug.Log("enter" + name);
            if (defaultState == null)
            {
                return;
            }
            ChangeState(defaultState.name);
        }
        public override void Exit()
        {
            base.Exit();
            if (CurrentState != null)
            {
                CurrentState.Exit();
                CurrentState = null;
            }
            Debug.Log("Exit" + name);
        }
        public override void Update()
        {
            if (isRunning == false || CurrentState == null)
                return;
            foreach (var i in activeTransitions)
            {
                if (CheckTransition(i))
                {
                    break;
                }
            }
            CurrentState.Update();

        }
        public override void FixUpdate()
        {
            if (isRunning == false || CurrentState == null)
                return;
            CurrentState.FixUpdate();
        }

    }
}
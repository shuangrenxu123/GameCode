using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class GoapPlanner
    {

        public Queue<GoapAction> Plan(GameObject gameObject, HashSet<GoapAction> availableActions, HashSet<KeyValuePair<string, object>> worldState
            , HashSet<KeyValuePair<string, object>> goal)
        {
            //重置所有行为
            foreach (var a in availableActions)
            {
                a.doReset();
            }
            //将可以执行的action放入里面
            HashSet<GoapAction> usebleActions = new HashSet<GoapAction>();
            foreach (var a in availableActions)
            {
                if (a.CheckProceduralPreconition(worldState))
                {
                    usebleActions.Add(a);
                }
            }
            List<Node> leaves = new List<Node>();
            Node start = new Node(null, 0, worldState, null);
            bool succes = BuildGraph(start, leaves, usebleActions, goal);

            if (!succes)
            {
                Debug.LogError("没有找到plan");
                return null;
            }
            Node cheapest = null;
            ///找出代价最小的一条路径
            foreach (Node leaf in leaves)
            {
                if (cheapest == null)
                {
                    cheapest = leaf;
                }
                else
                {
                    if (leaf.runningCost < cheapest.runningCost)
                    {
                        cheapest = leaf;
                    }
                }
            }

            List<GoapAction> result = new List<GoapAction>();
            Node n = cheapest;

            while (n != null)
            {
                if (n.action != null)
                {
                    result.Add(n.action);
                }
                n = n.parent;
            }
            //至此。我们获得了一个正确的action顺序
            Queue<GoapAction> queue = new();
            foreach (var i in result)
            {
                queue.Enqueue(i);
            }
            return queue;
        }
        /// <summary>
        /// 生成图
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="leaves"></param>
        /// <param name="usebleActions"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        private bool BuildGraph(Node parent, List<Node> leaves, HashSet<GoapAction> usebleActions, HashSet<KeyValuePair<string, object>> goal)
        {
            bool foundOnd = false;
            foreach (var action in usebleActions)
            {
                //前置条件也算单独的世界状态
                //if (InState(action.Preconditions, parent.state))
                //{
                //    //如果有，那我们尝试改变，然后返回执行后的世界状态。然后来尝试看看更改后的世界状态满不满足要求中的
                //    HashSet<KeyValuePair<string, object>> currentState = PopulateState(parent.state, action.Effects);
                //    //下一个节点。
                //    Node node = new Node(parent,parent.runningCost+action.cost,currentState,action);
                //    //目标世界状态和执行了一个的世界状态
                //    if (InState(goal,currentState))
                //    {
                //        leaves.Add(node);
                //        foundOnd = true;
                //    }
                //    else
                //    {
                //        //还没有解决方案，所以测试所有剩余的操作并分支出树
                //        HashSet<GoapAction> subset = ActionSubset(usebleActions,action);
                //        bool found = BuildGraph(node,leaves,subset,goal);
                //        if (found)
                //        {
                //            foundOnd = true;
                //        }
                //    }
                //}
                HashSet<KeyValuePair<string, object>> currentWorldState = PopulateState(parent.state, action.Effects);
                if (InState(goal, currentWorldState))
                {
                    Node node = new Node(parent, parent.runningCost + action.cost, currentWorldState, action);
                    if (!InState(action.Preconditions, currentWorldState))
                    {
                        HashSet<GoapAction> subset = ActionSubset(usebleActions, action);
                        bool found = BuildGraph(node, leaves, subset, action.Preconditions);
                        if (found)
                        {
                            foundOnd = true;
                        }
                    }
                    else
                    {
                        leaves.Add(node);
                        foundOnd = true;
                    }
                }
            }
            return foundOnd;
        }
        /// <summary>
        /// 创建除removeMe之外action作子集。创建新集合。
        /// </summary>
        /// <param name="usebleActions"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private HashSet<GoapAction> ActionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
        {
            ///子集
            HashSet<GoapAction> subset = new HashSet<GoapAction>();
            foreach (var a in actions)
            {
                if (!a.Equals(removeMe))
                {
                    subset.Add(a);
                }
            }
            return subset;
        }
        /// <summary>
        /// 应用世界状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="effects"></param>
        /// <returns></returns>
        private HashSet<KeyValuePair<string, object>> PopulateState(HashSet<KeyValuePair<string, object>> Currentstate, HashSet<KeyValuePair<string, object>> stateChange)
        {
            ///当前世界状态的副本
            HashSet<KeyValuePair<string, object>> state = new HashSet<KeyValuePair<string, object>>(Currentstate);

            foreach (var change in stateChange)
            {
                bool exits = false;
                //判断影响是否存在于世界状态
                foreach (var s in state)
                {
                    if (s.Equals(change))
                    {
                        exits = true;
                        break;
                    }
                }

                if (exits)
                {
                    state.RemoveWhere((KeyValuePair<string, object> kvp) => { return kvp.Key.Equals(change.Key); });
                    KeyValuePair<string, object> updated = new KeyValuePair<string, object>(change.Key, change.Value);
                    state.Add(updated);
                }
                else
                {
                    state.Add(new KeyValuePair<string, object>(change.Key, change.Value));
                }
            }
            return state;

        }
        ///<summary>
        /// 检查前置要求是否 可以靠世界状态满足，即世界状态是否存在这个条件
        /// 
        /// </summary>
        private bool InState(HashSet<KeyValuePair<string, object>> preconditions, HashSet<KeyValuePair<string, object>> state)
        {
            bool allMatch = true;
            foreach (var i in preconditions)
            {
                bool match = false;
                foreach (var s in state)
                {
                    if (s.Equals(i))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    allMatch = false;
                }
            }
            return allMatch;
        }
    }
    class Node
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public Node parent;
        /// <summary>
        /// 运行代价
        /// </summary>
        public float runningCost;
        /// <summary>
        /// 世界状态
        /// </summary>
        public HashSet<KeyValuePair<string, object>> state;
        /// <summary>
        /// 行为
        /// </summary>
        public GoapAction action;
        public Node(Node parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GoapAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }
}

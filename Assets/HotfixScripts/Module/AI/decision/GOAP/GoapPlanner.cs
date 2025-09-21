using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    class Node<T, V>
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public Node<T, V> parent;

        /// <summary>
        /// 运行时代价
        /// </summary>
        public float runningCost;

        public Dictionary<T, V> state;

        /// <summary>
        /// Action
        /// </summary>
        public GoapAction<T, V> action;

        public Node(Node<T, V> parent, float runningCost, Dictionary<T, V> state, GoapAction<T, V> action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }

    public class GoapPlanner<T, V>
    {

        HashSet<GoapAction<T, V>> usableActions = new();

        /// <summary>
        /// 叶子节点
        /// </summary>
        List<Node<T, V>> leavesNodes = new();

        /// <summary>
        /// 生成行动计划队列。过滤可用行动，构建状态图找到达到目标的最低代价路径。
        /// </summary>
        /// <param name="availableActions">所有可用行动集合</param>
        /// <param name="worldState">当前世界状态</param>
        /// <param name="goal">目标状态</param>
        /// <returns>如果找到路径，返回行动队列；否则返回null</returns>
        public Queue<GoapAction<T, V>> Plan(HashSet<GoapAction<T, V>> availableActions,
            Dictionary<T, V> worldState,
            Dictionary<T, V> goal)
        {
            usableActions.Clear();
            foreach (var a in availableActions)
            {
                if (a.CheckProceduralPreCondition(worldState))
                {
                    usableActions.Add(a);
                }
            }

            leavesNodes.Clear();
            Node<T, V> start = new(null, 0, worldState, null);
            bool success = BuildGraph(start, leavesNodes, usableActions, goal);

            if (!success)
            {
                Debug.LogError("【GOAP】没能成功找到一个可用的行为列表");
                return null;
            }

            Node<T, V> cheapestNode = null;
            //找到代价最小的节点
            foreach (Node<T, V> leaf in leavesNodes)
            {
                if (cheapestNode == null)
                {
                    cheapestNode = leaf;
                }
                else
                {
                    if (leaf.runningCost < cheapestNode.runningCost)
                    {
                        cheapestNode = leaf;
                    }
                }
            }

            List<GoapAction<T, V>> result = new List<GoapAction<T, V>>();
            Node<T, V> n = cheapestNode;

            while (n != null)
            {
                if (n.action != null)
                {
                    result.Add(n.action);
                }
                n = n.parent;
            }
            Queue<GoapAction<T, V>> queue = new();
            foreach (var i in result)
            {
                queue.Enqueue(i);
            }
            return queue;
        }

        /// <summary>
        /// 递归构建状态搜索图，探索行动序列直到达到目标状态。
        /// 使用深度优先搜索，剪枝行动，收集到达目标的叶子节点。
        /// </summary>
        /// <param name="parent">当前父节点</param>
        /// <param name="leaves">收集的叶子节点列表（到达目标的节点）</param>
        /// <param name="usableActions">当前可用的行动集合</param>
        /// <param name="goal">目标状态</param>
        /// <returns>如果找到路径，返回true</returns>
        private bool BuildGraph(Node<T, V> parent,
            List<Node<T, V>> leaves,
            HashSet<GoapAction<T, V>> usableActions,
            Dictionary<T, V> goal)
        {
            bool foundOnd = false;
            foreach (var action in usableActions)
            {
                if (InState(action.Preconditions, parent.state))
                {
                    Dictionary<T, V> currentState = PopulateState(parent.state, action.Effects);
                    Node<T, V> node = new(parent, parent.runningCost + action.cost, currentState, action);
                    if (InState(goal, currentState))
                    {
                        leaves.Add(node);
                        node.action.PlanEnter();
                        foundOnd = true;
                    }
                    else
                    {
                        HashSet<GoapAction<T, V>> subset = ActionSubset(usableActions, action);
                        bool found = BuildGraph(node, leaves, subset, goal);
                        if (found)
                        {
                            foundOnd = true;
                        }
                    }
                }

                Dictionary<T, V> currentWorldState = PopulateState(parent.state, action.Effects);
                if (InState(goal, currentWorldState))
                {
                    Node<T, V> node = new(parent, parent.runningCost + action.cost, currentWorldState, action);
                    if (!InState(action.Preconditions, currentWorldState))
                    {
                        HashSet<GoapAction<T, V>> subset = ActionSubset(usableActions, action);
                        bool found = BuildGraph(node, leaves, subset, action.Preconditions);
                        if (found)
                        {
                            foundOnd = true;
                        }
                    }
                    else
                    {
                        leaves.Add(node);
                        node.action.PlanEnter();
                        foundOnd = true;
                    }
                }
            }
            return foundOnd;
        }

        /// <summary>
        /// 从行动集合中移除指定行动，返回新的子集合，用于递归搜索中的action组合。
        /// </summary>
        /// <param name="actions">原始行动集合</param>
        /// <param name="removeMe">要移除的行动</param>
        /// <returns>移除指定行动后的新集合</returns>
        private HashSet<GoapAction<T, V>> ActionSubset(HashSet<GoapAction<T, V>> actions, GoapAction<T, V> removeMe)
        {
            HashSet<GoapAction<T, V>> subset = new();
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
        /// 根据行动的影响更新状态集。若状态键存在，则累加数值（支持增量）；若不存在，则添加新键值。
        /// 修改版本：假设V为int，支持增量计算而非覆盖。
        /// </summary>
        /// <param name="currentState">当前状态集</param>
        /// <param name="stateChange">待应用的状态变化集</param>
        /// <returns>更新后的新状态集</returns>
        private Dictionary<T, V> PopulateState
            (Dictionary<T, V> currentState,
            Dictionary<T, V> stateChange)
        {
            Dictionary<T, V> state = new Dictionary<T, V>(currentState);

            foreach (var change in stateChange)
            {
                if (state.ContainsKey(change.Key))
                {
                    // 假设 V 是 int，进行增量更新
                    if (typeof(V) == typeof(int))
                    {
                        int newVal = ((int)(object)state[change.Key]) + ((int)(object)change.Value);
                        state[change.Key] = (V)(object)newVal;
                    }
                    else
                    {
                        state[change.Key] = change.Value;
                    }
                }
                else
                {
                    state.Add(change.Key, change.Value);
                }
            }
            return state;
        }

        /// <summary>
        /// 检查前提条件是否在当前状态中完全匹配。
        /// 前提条件的每个键值对必须在状态集中存在并相等。
        /// </summary>
        /// <param name="preconditions">前提条件集</param>
        /// <param name="state">当前状态集</param>
        /// <returns>如果所有前提条件满足，返回true</returns>
        private bool InState(Dictionary<T, V> preconditions, Dictionary<T, V> state)
        {
            foreach (var kvp in preconditions)
            {
                if (!state.ContainsKey(kvp.Key) || !EqualityComparer<V>.Default.Equals(state[kvp.Key], kvp.Value))
                {
                    return false;
                }
            }
            return true;
        }
    }

}

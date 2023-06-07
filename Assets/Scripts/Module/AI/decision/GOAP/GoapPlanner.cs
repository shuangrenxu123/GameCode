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
            //����������Ϊ
            foreach (var a in availableActions)
            {
                a.doReset();
            }
            //������ִ�е�action��������
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
                Debug.LogError("û���ҵ�plan");
                return null;
            }
            Node cheapest = null;
            ///�ҳ�������С��һ��·��
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
            //���ˡ����ǻ����һ����ȷ��action˳��
            Queue<GoapAction> queue = new();
            foreach (var i in result)
            {
                queue.Enqueue(i);
            }
            return queue;
        }
        /// <summary>
        /// ����ͼ
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
                //ǰ������Ҳ�㵥��������״̬
                //if (InState(action.Preconditions, parent.state))
                //{
                //    //����У������ǳ��Ըı䣬Ȼ�󷵻�ִ�к������״̬��Ȼ�������Կ������ĺ������״̬��������Ҫ���е�
                //    HashSet<KeyValuePair<string, object>> currentState = PopulateState(parent.state, action.Effects);
                //    //��һ���ڵ㡣
                //    Node node = new Node(parent,parent.runningCost+action.cost,currentState,action);
                //    //Ŀ������״̬��ִ����һ��������״̬
                //    if (InState(goal,currentState))
                //    {
                //        leaves.Add(node);
                //        foundOnd = true;
                //    }
                //    else
                //    {
                //        //��û�н�����������Բ�������ʣ��Ĳ�������֧����
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
        /// ������removeMe֮��action���Ӽ��������¼��ϡ�
        /// </summary>
        /// <param name="usebleActions"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private HashSet<GoapAction> ActionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
        {
            ///�Ӽ�
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
        /// Ӧ������״̬
        /// </summary>
        /// <param name="state"></param>
        /// <param name="effects"></param>
        /// <returns></returns>
        private HashSet<KeyValuePair<string, object>> PopulateState(HashSet<KeyValuePair<string, object>> Currentstate, HashSet<KeyValuePair<string, object>> stateChange)
        {
            ///��ǰ����״̬�ĸ���
            HashSet<KeyValuePair<string, object>> state = new HashSet<KeyValuePair<string, object>>(Currentstate);

            foreach (var change in stateChange)
            {
                bool exits = false;
                //�ж�Ӱ���Ƿ����������״̬
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
        /// ���ǰ��Ҫ���Ƿ� ���Կ�����״̬���㣬������״̬�Ƿ�����������
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
        /// ���ڵ�
        /// </summary>
        public Node parent;
        /// <summary>
        /// ���д���
        /// </summary>
        public float runningCost;
        /// <summary>
        /// ����״̬
        /// </summary>
        public HashSet<KeyValuePair<string, object>> state;
        /// <summary>
        /// ��Ϊ
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

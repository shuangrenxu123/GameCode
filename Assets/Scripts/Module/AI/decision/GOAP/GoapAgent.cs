using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class GoapAgent : MonoBehaviour
    {
        private HashSet<GoapAction> availabActions;

        private Queue<GoapAction> currentActions;

        private IGoap dataProvider;

        private GoapPlanner planner;

        private bool Runing = false;
        private void Start()
        {
            availabActions = new HashSet<GoapAction>();
            currentActions = new Queue<GoapAction>();
            planner = new GoapPlanner();
            findDataProvider();
            InitAction();
        }

        private void InitAction()
        {
            availabActions = dataProvider.InitAction();
        }

        private void Update()
        {
            if (BuildPlan() && Runing == false)
            {
                Runing = true;
            }
            if (Runing)
            {
                Runing = !RunPlan();
            }

        }
        private void findDataProvider()
        {
            foreach (Component comp in gameObject.GetComponents(typeof(Component)))
            {
                if (typeof(IGoap).IsAssignableFrom(comp.GetType()))
                {
                    dataProvider = (IGoap)comp;
                    return;
                }
            }
        }

        private bool HasActionPlan()
        {
            return currentActions.Count > 0;
        }

        /// <summary>
        /// �Ƿ��ҵ��˺��ʵļƻ�
        /// </summary>
        /// <returns></returns>
        private bool BuildPlan()
        {
            HashSet<KeyValuePair<string, object>> worldState = dataProvider.GetWorldState();

            HashSet<Goal> goals = dataProvider.CreateGoalState();
            Goal goal = null;
            foreach (var g in goals)
            {
                if (goal == null)
                {
                    goal = g;
                }
                else
                {
                    if (goal.Priority < g.Priority)
                    {
                        goal = g;
                    }
                }
            }

            //��� Actions
            Queue<GoapAction> plan = planner.Plan(gameObject, availabActions, worldState, goal.goal);
            if (plan != null)
            {
                currentActions = plan;
                dataProvider.planFound(goal.goal, plan);
                return true;
            }
            else
            {
                dataProvider.planFailed(goal.goal);
                return false;
            }
        }
        /// <summary>
        /// ִ��plan
        /// </summary>
        /// <returns>�Ƿ��������</returns>
        private bool RunPlan()
        {
            if (!HasActionPlan())
            {
                dataProvider.ActionFinished();
                return true;
            }
            GoapAction action = currentActions.Peek();
            if (action.isDone())
            {
                currentActions.Dequeue();
            }
            if (HasActionPlan())
            {
                action = currentActions.Peek();
                bool sucees = action.Perform(gameObject);
                if (!sucees)
                {
                    dataProvider.planAborted(action);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                dataProvider.ActionFinished();
                return true;
            }
        }
    }
}

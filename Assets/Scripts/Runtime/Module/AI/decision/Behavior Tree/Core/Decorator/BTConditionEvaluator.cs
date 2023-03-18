using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT {
    /// <summary>
    /// 该修饰节点用于判断前置条件，我们可以通过该节点，并添加一系列的前置条件来控制返回Susseecs或者Faild
    /// </summary>
    public class BTConditionEvaluator : BTDecorator
    {
        private List<BTConditional> conditionals;
        public BTLogic logicOpt;
        /// <summary>
        /// 是否每次Tick都要执行检测前置条件
        /// </summary>
        public bool reevaludateEveryTick;
        public ClearChildOpt clearopt;

        private BTResult _previousResult = BTResult.Failed;

        public BTConditionEvaluator(List<BTConditional> conditionals,BTLogic logicOpt, bool reevaludateEveryTick, ClearChildOpt clearopt,BTNode child) :base(child)
        {
            this.conditionals = conditionals;
            this.logicOpt = logicOpt;
            this.reevaludateEveryTick = reevaludateEveryTick;
            this.clearopt = clearopt;
        }

        public BTConditionEvaluator(BTLogic logicOpt, bool reevaludateEveryTick, ClearChildOpt clearopt,BTNode child) :base(child)
        {
            conditionals = new List<BTConditional>();
            this.logicOpt = logicOpt;
            this.reevaludateEveryTick = reevaludateEveryTick;
            this.clearopt = clearopt;
        }

        public override void Activate(BTDataBase database)
        {
            base.Activate(database);
            foreach (var cond in conditionals)
            {
                cond.Activate(database);
            }
        }

        public override BTResult Tick()
        {
            //先判断是否满足前置条件
            if(_previousResult != BTResult.Running || reevaludateEveryTick)
            {
                if(logicOpt == BTLogic.And)
                {
                    int i = 0;
                    foreach (var cond in conditionals)
                    {
                        i++;
                        if(!cond.Check())
                        {
                            return BTResult.Failed;
                        }
                    }
                }
                else
                {
                    bool anySuccess = false;
                    int i = 0;
                    foreach (var cond in conditionals)
                    {
                        i++;
                        if(cond.Check())
                        {
                            anySuccess = true;
                            break;
                        }
                    }

                    if(!anySuccess)
                    {
                        return BTResult.Failed;
                    }
                }
            }
            _previousResult = child.Tick();
            if(_previousResult == BTResult.Running)
            {
                isRunning = true;
            }
            return _previousResult;
        }
        public override void Clear()
        {
            if((isRunning && clearopt == ClearChildOpt.OnAbortRunning) ||
                (_previousResult == BTResult.Success && clearopt == ClearChildOpt.OnStopRunning)||
                clearopt == ClearChildOpt.OnNotRunning
                )
            {
                isRunning= false;
                child.Clear();
            }
            if(clearTick != null)
            {
                clearTick.Clear();
            }
            _previousResult= BTResult.Failed;

        }

        public void AddConditional(BTConditional conditional)
        {
            if (!conditionals.Contains(conditional))
            {
                conditionals.Add(conditional);
            }
        }

        public void RemoveConditional(BTConditional conditional)
        {
            int index = conditionals.IndexOf(conditional);
            conditionals.Remove(conditional);
        }
    }

    public enum ClearChildOpt
    {
        OnAbortRunning,
        OnStopRunning,
        OnNotRunning,
    }
}
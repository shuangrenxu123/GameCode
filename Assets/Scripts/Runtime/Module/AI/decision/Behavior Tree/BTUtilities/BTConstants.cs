using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public enum BTResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 错误
        /// </summary>
        Failed,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
    }

    public enum BTAbortOpt
    {
        None,
        Self,
        LowerPriority,
        Both,
    }

    /// <summary>
    /// 清除类型
    /// </summary>
    public enum BTClearOpt
    {
        Default,
        Selected,
        DefaultAndSelected,
        All,
    }

    public enum BTLogic
    {
        /// <summary>
        /// 全都要满足
        /// </summary>
        And,
        /// <summary>
        /// 满足一个即可
        /// </summary>
        Or,
    }

    public enum BTExecuteOpt
    {
        OnTick,
        OnClear,
        Both,
    }

    public enum BTDataReadOpt
    {
        ReadAtBeginning,
        ReadEveryTick,
    }
    public enum ParallelType
    {

        /// <summary>
        /// 等待所有的子节点都执行完
        /// </summary>
        And = 1,
        /// <summary>
        /// 只要有一个提前整完
        /// </summary>
        Or = 2,
    }
}

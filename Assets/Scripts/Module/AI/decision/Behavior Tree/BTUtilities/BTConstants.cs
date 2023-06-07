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

    public enum ParallelType
    {

        /// <summary>
        /// 等待所有的子节点都执行完，提前执行完毕的节点不会再重复执行
        /// </summary>
        And = 1,
        /// <summary>
        /// 只要有一个提前整完
        /// </summary>
        Or = 2,
        /// <summary>
        /// 等待所有节点同时完成，如果有一个未完成，其他的会重复执行
        /// </summary>
        Wait=3,
    }
}

using AIBlackboard;

namespace BT
{
    /// <summary>
    /// 所有节点的父节点
    /// </summary>
    public class BTNode
    {
        public bool isRunning { get; set; }


        public Blackboard database;

        /// <summary>
        /// 激活节点,可以视作是节点添加完成后调用,此时已经获得数据库
        /// </summary>
        /// <param name="database"></param>
        public virtual void Activate(Blackboard database)
        {
            this.database = database;

        }
        /// <summary>
        /// 清除只用关心子节点的清除
        /// </summary>
        public virtual void Clear()
        {

        }

        public virtual BTResult Tick()
        {
            return BTResult.Success;
        }

    }
}

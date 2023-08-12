namespace BT
{
    /// <summary>
    /// 修饰器节点，只允许拥有一个子节点
    /// </summary>
    public class BTDecorator : BTNode
    {
        public BTNode child;
        public BTDecorator(BTNode child)
        {
            this.child = child;
        }
        public override void Activate(DataBase database)
        {
            base.Activate(database);
            child.Activate(database);
        }
        public override void Clear()
        {
            base.Clear();
            child.Clear();
        }
    }
}
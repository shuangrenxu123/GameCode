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
        public override void Activate(DataBase database, Enemy e)
        {
            base.Activate(database, e);
            child.Activate(database, e);
        }
        public override void Clear()
        {
            base.Clear();
            child.Clear();
        }
    }
}
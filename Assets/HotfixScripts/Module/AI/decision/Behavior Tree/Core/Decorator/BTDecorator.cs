namespace BT
{
    /// <summary>
    /// 修饰器节点，只允许拥有一个子节点
    /// </summary>
    public class BTDecorator<TKey, TValue> : BTNode<TKey, TValue>
    {
        public BTNode<TKey, TValue> child;
        public BTDecorator(BTNode<TKey, TValue> child)
        {
            this.child = child;
        }
        public override void Activate(DataBase<TKey, TValue> database)
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
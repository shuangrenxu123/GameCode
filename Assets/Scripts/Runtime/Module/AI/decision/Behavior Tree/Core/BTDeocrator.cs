namespace BT
{
    /// <summary>
    /// 对节点返回以后的处理进行处理，比如做一个延时器或者循环器
    /// </summary>
    public abstract class BTDeocrator : BTNode
    {
        public BTDeocrator() : base(null)
        {

        }
        public override BTResult Tick()
        {
            BTResult result = children[0].Tick();
            return Solve(result);
        }
        public abstract BTResult Solve(BTResult result);
    }
}

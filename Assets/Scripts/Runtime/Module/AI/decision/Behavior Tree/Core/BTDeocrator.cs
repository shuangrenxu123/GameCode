namespace BT
{
    /// <summary>
    /// �Խڵ㷵���Ժ�Ĵ�����д���������һ����ʱ������ѭ����
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

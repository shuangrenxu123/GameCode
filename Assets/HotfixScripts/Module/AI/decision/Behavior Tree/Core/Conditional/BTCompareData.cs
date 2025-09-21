namespace BT
{
    /// <summary>
    ///
    /// 它将提供的数据与BTDatabase中的数据进行比较。
  /// 如果它们相等，则返回 true，否则返回 false。
    /// </summary>
    public class BTCompareData<TKey, TValue> : BTConditional<TKey, TValue>
    {
        private TKey readDataName;
        private TValue rhs;

        public BTCompareData(TKey readDataName, TValue rhs, BTNode<TKey, TValue> child) : base(child)
        {
            this.readDataName = readDataName;
            this.rhs = rhs;
        }
        public override bool Check()
        {
            if (rhs == null)
            {
                return database.CheckDataNull(readDataName);
            }
            return database.GetData<TValue>(readDataName).Equals(rhs);
        }
    }
}
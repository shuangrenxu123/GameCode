namespace BT
{
    /// <summary>
    ///
    /// 它将提供的数据与BTDatabase中的数据进行比较。
    /// 如果它们相等，则返回 true，否则返回 false。
    /// </summary>
    public class BTCompareData : BTConditional
    {
        private string readDataName;
        private object rhs;

        public BTCompareData(string readDataName, object rhs, BTNode child) : base(child)
        {
            this.readDataName = readDataName;
            this.rhs = rhs;
        }
        public override bool Check()
        {
            if (rhs == null)
            {
                return database.ContainsData<object>(readDataName);
            }
            return database.GetValue<object>(readDataName).Equals(rhs);
        }
    }
}
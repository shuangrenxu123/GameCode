namespace BT
{
    /// <summary>
    ///
    /// 它将提供的数据与BTDatabase中的数据进行比较。
	/// 如果它们相等，则返回 true，否则返回 false。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BTCompareData<T> : BTConditional
    {
        private string readDataName;
        private int readDataId;
        private T rhs;

        public BTCompareData(string readDataName, T rhs, BTNode child) : base(child)
        {
            this.readDataName = readDataName;
            this.rhs = rhs;
        }
        public override bool Check()
        {
            if (rhs == null)
            {
                return database.CheckDataNull(readDataId);
            }
            return database.GetData<T>(readDataId).Equals(rhs);
        }
    }
}
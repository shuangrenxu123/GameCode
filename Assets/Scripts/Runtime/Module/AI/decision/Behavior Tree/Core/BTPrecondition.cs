namespace BT
{
    /// <summary>
    /// BT前提条件用于检查是否可以输入BTNode。
	/// 从BTNode继承意味着它也可以用作普通节点，
	/// 当您需要检查某些条件以结束某些逻辑时，它很有用
    /// 其中很难向操作/逻辑节点添加前提条件（由于重用问题）。
    /// </summary>
    public abstract class BTPrecondition : BTNode
    {
        public BTPrecondition() : base(null)
        {

        }
        public abstract bool Check();

        public override BTResult Tick()
        {
            bool success = Check();
            if (success)
            {
                return BTResult.Ended;
            }
            else
            {
                return BTResult.Running;
            }
        }

    }
    /// <summary>
    /// 使用数据库的前提条件
    /// </summary>
    public abstract class BTPreconditionUseDB : BTPrecondition
    {
        protected string dataToCheak;
        protected int dataIdToCheak;
        public BTPreconditionUseDB(string dataToCheak)
        {
            this.dataToCheak = dataToCheak;
        }
        public override void Activate(BTDataBase database)
        {
            base.Activate(database);
            dataIdToCheak = database.GetDataId(dataToCheak);
        }
    }

    /// <summary>
    /// 用于检查黑板中的浮点数数据是否小于/等于/大于通过构造函数传入的数据。用于判断节点是否进入使用？
    /// 
    /// </summary>
    public abstract class BTPreconditionFloat : BTPreconditionUseDB
    {
        public float rhs;
        public FloatFunction func;
        protected BTPreconditionFloat(string dataToCheak, float rhs, FloatFunction func) : base(dataToCheak)
        {
            this.func = func;
            this.rhs = rhs;
        }
        public override bool Check()
        {
            float lhs = database.GetData<float>(dataIdToCheak);

            switch (func)
            {
                case FloatFunction.lessThan:
                    return lhs < rhs;
                case FloatFunction.GreaterThan:
                    return lhs > rhs;
                case FloatFunction.EqualTo:
                    return lhs == rhs;
            }
            return false;
        }
        public enum FloatFunction
        {
            /// <summary>
            /// 小于
            /// </summary>
            lessThan = 1,
            /// <summary>
            /// 大于
            /// </summary>
            GreaterThan = 2,
            /// <summary>
            /// 等于
            /// </summary>
            EqualTo = 3,
        }
    }
    /// <summary>
    /// 用于检查对比黑板中的某个布尔值
    /// </summary>
    public abstract class BTPreconditionBool : BTPreconditionUseDB
    {
        public bool rhs;
        protected BTPreconditionBool(string dataToCheak, bool rhs) : base(dataToCheak)
        {
            this.rhs = rhs;
        }
        public override bool Check()
        {
            bool lhs = database.GetData<bool>(dataIdToCheak);
            return lhs == rhs;
        }
    }
}
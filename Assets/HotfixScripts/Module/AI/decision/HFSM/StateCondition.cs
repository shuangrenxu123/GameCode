using AIBlackboard;

namespace HFSM
{
    public abstract class StateCondition
    {
        public string dataName;
        protected Blackboard dataBase;
        public abstract bool Check();
        public StateCondition(string name, Blackboard dataBase)
        {
            dataName = name;
            this.dataBase = dataBase;

        }
    }
    public class StateCondition_Bool : StateCondition
    {
        /// <summary>
        /// 期望值
        /// </summary>
        private bool condition;
        public StateCondition_Bool(string name, Blackboard dataBase, bool cond) : base(name, dataBase)
        {
            condition = cond;
        }
        public override bool Check()
        {
            if (dataBase.GetValue<bool>(dataName) == condition)
            {
                return true;
            }
            return false;

        }
    }
    public class StateCondition_Float : StateCondition
    {
        private float condition;
        private FloatOpt opt;
        public StateCondition_Float(string name, Blackboard dataBase, float cond, FloatOpt opt) : base(name, dataBase)
        {
            condition = cond;
            this.opt = opt;
        }
        public override bool Check()
        {
            float value = dataBase.GetValue<float>(dataName);
            switch (opt)
            {
                case FloatOpt.lt:
                    return value < condition;
                case FloatOpt.eq:
                    return value == condition;
                case FloatOpt.gt:
                    return value > condition;
            }
            return false;
        }
    }
}
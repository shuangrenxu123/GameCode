namespace BT
{
    public class BTinterval<TKey, TValue> : BTDecorator<TKey, TValue>
    {
        public float interval;
        private TKey dataBaseName;
        public BTinterval(TKey databaseName, float time, BTNode<TKey, TValue> child) : base(child)
        {
            dataBaseName = databaseName;
            interval = time;
        }
        public override void Activate(DataBase<TKey, TValue> database)
        {
            base.Activate(database);
            database.SetData(dataBaseName, 0f);
        }
        public override BTResult Tick()
        {
            var time = database.GetData<float>(dataBaseName);
            if (time <= interval)
            {
                return BTResult.Failed;
            }
            else
            {
                var result = child.Tick();
                if (result != BTResult.Running)
                {
                    database.SetData(dataBaseName, 0f);
                    return result;
                }
                else
                {
                    return BTResult.Running;
                }
            }
        }

    }
}
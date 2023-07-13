namespace BT
{
    public class BTinterval : BTDecorator
    {
        public float interval;
        private string dataBaseName;
        public BTinterval(string databaseName, float time, BTNode child) : base(child)
        {
            dataBaseName = databaseName;
            interval = time;
        }
        public override void Activate(DataBase database, Enemy e)
        {
            base.Activate(database, e);
            database.SetData<float>(dataBaseName, 0);
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
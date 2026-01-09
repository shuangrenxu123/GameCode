using AIBlackboard;

namespace BT
{
    [BT.EditorIntegration.BTEditorNode("Decorator/Interval", BT.EditorIntegration.BTEditorNodeKind.Decorator)]
    public class BTinterval : BTDecorator
    {
        public float interval;
        private string dataBaseName;
        [BT.EditorIntegration.BTEditorConstructor]
        public BTinterval(string databaseName, float time, BTNode child) : base(child)
        {
            dataBaseName = databaseName;
            interval = time;
        }
        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            database.SetValue(dataBaseName, 0f);
        }
        public override BTResult Tick()
        {
            var time = database.GetValue<string, float>(dataBaseName);
            if (time <= interval)
            {
                return BTResult.Failed;
            }
            else
            {
                var result = child.Tick();
                if (result != BTResult.Running)
                {
                    database.SetValue(dataBaseName, 0f);
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

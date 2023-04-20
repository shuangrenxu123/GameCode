using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace BT
{
    public class BTinterval : BTDecorator
    {
        //private float timer = 0;
        public float interval;
        private string dataBaseName;
        public BTinterval(string databaseName,float time, BTNode child) : base(child)
        {
            dataBaseName = databaseName;
            interval = time;
        }
        public override void Activate(BTDataBase database)
        {
            base.Activate(database);
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
                if(result != BTResult.Running)
                {
                    database.SetData(dataBaseName,0f);
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
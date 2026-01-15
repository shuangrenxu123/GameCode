using System;
using System.Collections.Generic;

namespace BT.RuntimeSerialization
{
    [Serializable]
    public class BTTreeRuntimeJson
    {
        public int version = 1;
        public string rootId;
        public List<BTBlackboardEntryJson> blackboard = new();
        public List<BTNodeRuntimeJson> nodes = new();
    }

    [Serializable]
    public class BTNodeRuntimeJson
    {
        public string id;
        public string typeId;
        public List<BTArgJson> args = new();
        public List<string> children = new();
    }

    public enum BTArgType
    {
        String = 0,
        Int = 1,
        Float = 2,
        Bool = 3,
        Vector2 = 4,
        Vector3 = 5,
    }

    [Serializable]
    public class BTArgJson
    {
        public string name;
        public BTArgType type;
        public string value;
    }

    public enum BTBlackboardValueType
    {
        String = 0,
        Int = 1,
        Float = 2,
        Bool = 3,
    }

    [Serializable]
    public class BTBlackboardEntryJson
    {
        public string key;
        public BTBlackboardValueType type;
        public string value;
    }
}

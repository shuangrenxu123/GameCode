using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace HTN
{
    /// <summary>
    /// 基本的的世界状态信息。
    /// </summary>
    public enum WSProperties
    {
        /// <summary>
        /// 所在位置（Enum）
        /// </summary>
        WS_Location,
        /// <summary>
        /// 是否可以看见敌人（Bool）
        /// </summary>
        WS_CanSeeEnemy,
        /// <summary>
        /// 树干（武器）耐久度（Int）
        /// </summary>
        WS_TrunkHealth,
        /// <summary>
        /// 最近是否看见过敌人（Bool）
        /// </summary>
        WS_HasSeenEnemyRecently,
        /// <summary>
        /// 近期是否攻击过（Bool）
        /// </summary>
        WS_AttackedRecently,
        /// <summary>
        /// 是否可寻路到敌人位置（Bool）
        /// </summary>
        WS_CanNavigateToEnemy,
    }
    public enum Location
    {
        /// <summary>
        /// 在敌人附近
        /// </summary>
        EnemyLocRef,
        /// <summary>
        /// 桥附近
        /// </summary>
        NextBridgeLocRef,
        /// <summary>
        /// 树干附近
        /// </summary>
        FoundTrunk,
        /// <summary>
        /// 最近一次敌人的位置
        /// </summary>
        LastEnemyPos,
    }
    public class WorldState
    {
        public WorldState()
        {
            Reset();
        }
        /// <summary>
        /// 世界状态的缓存
        /// </summary>
        private Dictionary<WSProperties, Enum> wsEnum;
        private Dictionary<WSProperties, int> wsInt;
        private Dictionary<WSProperties, float> wsFloat;
        private Dictionary<WSProperties, bool> wsBool;
        private Dictionary<WSProperties, object> wsObj;

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            wsEnum = new Dictionary<WSProperties, Enum>();
            wsInt = new Dictionary<WSProperties, int>();
            wsFloat = new Dictionary<WSProperties, float>();
            wsBool = new Dictionary<WSProperties, bool>();
            wsObj = new Dictionary<WSProperties, object>();

            Add(WSProperties.WS_CanSeeEnemy, true);
            Add(WSProperties.WS_Location, Location.NextBridgeLocRef);
            Add(WSProperties.WS_TrunkHealth, 0);
            Add(WSProperties.WS_AttackedRecently, false);
            Add(WSProperties.WS_CanNavigateToEnemy, true);

        }
        #region Add:添加世界状态属性
        public void Add(WSProperties p, Enum v)
        {
            wsEnum[p] = v;
        }
        public void Add(WSProperties p, int v)
        {
            wsInt[p] = v;
        }
        public void Add(WSProperties p, float v)
        {
            wsFloat[p] = v;
        }
        public void Add(WSProperties p, bool v)
        {
            wsBool[p] = v;
        }
        public void Add(WSProperties p, object v)
        {
            wsObj[p] = v;
        }
        #endregion
        #region Get:获得世界状态属性
        public Enum GetEnum(WSProperties p)
        {
            return wsEnum[p];
        }
        public int GetInt(WSProperties p)
        {
            return wsInt[p];
        }
        public bool GetBool(WSProperties p)
        {
            return wsBool[p];
        }
        public float GetFloat(WSProperties p)
        {
            return wsFloat[p];
        }
        public object GetObj(WSProperties p)
        {
            return wsObj[p];
        }
        #endregion
        #region Set:设置世界状态属性
        /// <summary>
        /// 设置世界状态属性
        /// </summary>
        /// <param name="propType">属性枚举</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public bool Set(WSProperties propType, Enum value)
        {
            if (wsEnum.ContainsKey(propType))
            {
                wsEnum[propType] = value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置世界状态属性
        /// </summary>
        /// <param name="propType">属性枚举</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public bool Set(WSProperties propType, int value)
        {
            if (wsInt.ContainsKey(propType))
            {
                wsInt[propType] = value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置世界状态属性
        /// </summary>
        /// <param name="propType">属性枚举</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public bool Set(WSProperties propType, bool value)
        {
            if (wsBool.ContainsKey(propType))
            {
                wsBool[propType] = value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置世界状态属性
        /// </summary>
        /// <param name="propType">属性枚举</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public bool Set(WSProperties propType, float value)
        {
            if (wsFloat.ContainsKey(propType))
            {
                wsFloat[propType] = value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置世界状态属性
        /// </summary>
        /// <param name="propType">属性枚举</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public bool Set(WSProperties propType, object value)
        {
            if (wsObj.ContainsKey(propType))
            {
                wsObj[propType] = value;
                return true;
            }
            return false;
        }
        #endregion
        #region Contains：是否包含某些条件
        /// <summary>
        /// 是否包含某些条件
        /// </summary>
        /// <param name="cond"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<WSProperties, Enum> cond)
        {
            if (wsEnum.ContainsKey(cond.Key) == false || wsEnum[cond.Key] != cond.Value)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 是否包含某些条件
        /// </summary>
        /// <param name="cond"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<WSProperties, int> cond)
        {
            if (wsInt.ContainsKey(cond.Key) == false || wsInt[cond.Key] != cond.Value)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 是否包含某些条件
        /// </summary>
        /// <param name="cond"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<WSProperties, bool> cond)
        {
            if (wsBool.ContainsKey(cond.Key) == false || wsBool[cond.Key] != cond.Value)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 是否包含某些条件
        /// </summary>
        /// <param name="cond"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<WSProperties, float> cond)
        {
            if (wsFloat.ContainsKey(cond.Key) == false || wsFloat[cond.Key] != cond.Value)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 是否包含某些条件
        /// </summary>
        /// <param name="cond"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<WSProperties, object> cond)
        {
            if (wsObj.ContainsKey(cond.Key) == false || wsObj[cond.Key] != cond.Value)
            {
                return false;
            }
            return true;
        }
        #endregion
        /// <summary>
        /// 返回一个世界状态的副本
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public void CopyFrom(WorldState ws)
        {
            WorldState w = new WorldState();
            foreach (var item in ws.wsEnum)
            {
                Add(item.Key, item.Value);
            }
            foreach (var item in ws.wsInt)
            {
                Add(item.Key, item.Value);
            }
            foreach (var item in ws.wsBool)
            {
                Add(item.Key, item.Value);
            }
            foreach (var item in ws.wsFloat)
            {
                Add(item.Key, item.Value);
            }
            foreach (var item in ws.wsObj)
            {
                Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 打印世界状态（测试使用）
        /// </summary>
        public void LogWorldState(string title)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in wsEnum)
            {
                sb.Append("//世界属性：").Append(item.Key.ToString()).Append("--属性值：").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsInt)
            {
                sb.Append("//世界属性：").Append(item.Key.ToString()).Append("--属性值：").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsBool)
            {
                sb.Append("//世界属性：").Append(item.Key.ToString()).Append("--属性值：").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsFloat)
            {
                sb.Append("//世界属性：").Append(item.Key.ToString()).Append("--属性值：").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsObj)
            {
                sb.Append("//世界属性：").Append(item.Key.ToString()).Append("--属性值：").Append(item.Value.ToString()).Append("//\n");
            }
            Debug.LogWarning(title + "  LogWorldState：\n" + sb.ToString());
        }
    }
}

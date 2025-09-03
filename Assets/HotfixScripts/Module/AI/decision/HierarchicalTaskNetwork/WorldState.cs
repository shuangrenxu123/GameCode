using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace HTN
{
    /// <summary>
    /// 世界的世界状态信息
    /// </summary>
    public enum WSProperties
    {
        /// <summary>
        /// 当前位置，Enum类型
        /// </summary>
        WS_Location,

        /// <summary>
        /// 是否能看见敌人，Bool类型
        /// </summary>
        WS_CanSeeEnemy,

        /// <summary>
        /// 树干健康度，Int类型
        /// </summary>
        WS_TrunkHealth,

        /// <summary>
        /// 是否最近看见敌人，Bool类型
        /// </summary>
        WS_HasSeenEnemyRecently,

        /// <summary>
        /// 是否最近被攻击，Bool类型
        /// </summary>
        WS_AttackedRecently,

        /// <summary>
        /// 是否能导航到敌人位置，Bool类型
        /// </summary>
        WS_CanNavigateToEnemy,

        /// <summary>
        /// 农夫体力值，Int类型
        /// </summary>
        WS_FarmerStamina,

        /// <summary>
        /// 木材数量，Int类型
        /// </summary>
        WS_WoodCount,

        /// <summary>
        /// 斧头耐久度，Int类型
        /// </summary>
        WS_AxeDurability,
    }

    public enum Location
    {
        /// <summary>
        /// 敌人附近
        /// </summary>
        EnemyLocRef,

        /// <summary>
        /// 桥附近
        /// </summary>
        NextBridgeLocRef,

        /// <summary>
        /// 找到的树干
        /// </summary>
        FoundTrunk,

        /// <summary>
        /// 最后一次敌人的位置
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
        /// 世界状态的存储
        /// </summary>
        private Dictionary<WSProperties, Enum> wsEnum;
        private Dictionary<WSProperties, int> wsInt;
        private Dictionary<WSProperties, float> wsFloat;
        private Dictionary<WSProperties, bool> wsBool;
        private Dictionary<WSProperties, object> wsObj;

        /// <summary>
        /// 初始化设置
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

            // 农夫Demo的初始状态
            Add(WSProperties.WS_FarmerStamina, 100);  // 初始体力100
            Add(WSProperties.WS_WoodCount, 0);       // 初始木材0
            Add(WSProperties.WS_AxeDurability, 10);  // 初始斧头耐久10
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

        #region Get:获取世界状态属性
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
        /// 设置枚举类型的世界状态属性
        /// </summary>
        /// <param name="propType">属性类型</param>
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
        /// 设置int类型的世界状态属性
        /// </summary>
        /// <param name="propType">属性类型</param>
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
        /// 设置bool类型的世界状态属性
        /// </summary>
        /// <param name="propType">属性类型</param>
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
        /// 设置float类型的世界状态属性
        /// </summary>
        /// <param name="propType">属性类型</param>
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
        /// 设置object类型的世界状态属性
        /// </summary>
        /// <param name="propType">属性类型</param>
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

        #region Contains检查是否包含某些属性
        /// <summary>
        /// 是否包含某些属性
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
        /// 是否包含某些属性
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
        /// 是否包含某些属性
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
        /// 是否包含某些属性
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
        /// 是否包含某些属性
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
        /// 从另一个世界状态复制
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
        /// 打印世界状态信息用于调试
        /// </summary>
        public void LogWorldState(string title)
        {
            StringBuilder sb = new StringBuilder();
            if (wsEnum != null)
            {
                foreach (var item in wsEnum)
                {
                    sb.Append("//状态属性：").Append(item.Key.ToString()).Append("--状态值：").Append(item.Value?.ToString()).Append("//\n");
                }
            }
            if (wsInt != null)
            {
                foreach (var item in wsInt)
                {
                    sb.Append("//状态属性：").Append(item.Key.ToString()).Append("--状态值：").Append(item.Value.ToString()).Append("//\n");
                }
            }
            if (wsBool != null)
            {
                foreach (var item in wsBool)
                {
                    sb.Append("//状态属性：").Append(item.Key.ToString()).Append("--状态值：").Append(item.Value.ToString()).Append("//\n");
                }
            }
            if (wsFloat != null)
            {
                foreach (var item in wsFloat)
                {
                    sb.Append("//状态属性：").Append(item.Key.ToString()).Append("--状态值：").Append(item.Value.ToString()).Append("//\n");
                }
            }
            if (wsObj != null)
            {
                foreach (var item in wsObj)
                {
                    sb.Append("//状态属性：").Append(item.Key.ToString()).Append("--状态值：").Append(item.Value?.ToString()).Append("//\n");
                }
            }
            // 显示精简的状态概览
            string toolStatus = GetInt(WSProperties.WS_AxeDurability) > 0
                ? $"斧头(耐默: {GetInt(WSProperties.WS_AxeDurability)}/10)"
                : "徒手(无斧头)";

            Debug.LogWarning($"{title} 状态 | 体:{GetInt(WSProperties.WS_FarmerStamina)} | 木:{GetInt(WSProperties.WS_WoodCount)} | 工具:{toolStatus}");
            // 如果需要详细日志，可以取消注释下面的行
            // Debug.Log(sb.ToString());
        }
    }
}

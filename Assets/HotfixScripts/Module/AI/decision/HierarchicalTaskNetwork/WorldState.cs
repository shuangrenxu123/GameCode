using System;
using System.Collections.Generic;
using System.Text;
using AIBlackboard;
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
        private Blackboard db = new();

        public WorldState()
        {
            Reset();
        }

        /// <summary>
        /// 初始化设置
        /// </summary>
        public void Reset()
        {
            db.Reset();

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
            db.SetValue(p, v);
        }
        public void Add(WSProperties p, int v)
        {
            db.SetValue(p, v);
        }
        public void Add(WSProperties p, float v)
        {
            db.SetValue(p, v);
        }
        public void Add(WSProperties p, bool v)
        {
            db.SetValue(p, v);
        }
        public void Add(WSProperties p, object v)
        {
            db.SetValue(p, v);
        }
        #endregion

        #region Get:获取世界状态属性
        public Enum GetEnum(WSProperties p)
        {
            return (Enum)db.GetValue<WSProperties, Enum>(p);
        }
        public int GetInt(WSProperties p)
        {
            return (int)db.GetValue<WSProperties, int>(p);
        }
        public bool GetBool(WSProperties p)
        {
            return (bool)db.GetValue<WSProperties, bool>(p);
        }
        public float GetFloat(WSProperties p)
        {
            return db.GetValue<WSProperties, float>(p);
        }
        public object GetObj(WSProperties p)
        {
            return db.GetValue<WSProperties, object>(p);
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
            if (db.ContainsData<WSProperties, Enum>(propType))
            {
                db.SetValue(propType, value);
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
            if (db.ContainsData<WSProperties, int>(propType))
            {
                db.SetValue(propType, value);
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
            if (db.ContainsData<WSProperties, bool>(propType))
            {
                db.SetValue(propType, value);
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
            if (db.ContainsData<WSProperties, float>(propType))
            {
                db.SetValue(propType, value);
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
            if (db.ContainsData<WSProperties, object>(propType))
            {
                db.SetValue(propType, value);
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
            if (!db.ContainsData<WSProperties, Enum>(cond.Key) || db.GetValue<object, Enum>(cond.Key) != cond.Value)
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
            if (!db.ContainsData<WSProperties, int>(cond.Key) || (int)db.GetValue<WSProperties, int>(cond.Key) != cond.Value)
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
            if (!db.ContainsData<WSProperties, bool>(cond.Key) || (bool)db.GetValue<WSProperties, bool>(cond.Key) != cond.Value)
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
            if (!db.ContainsData<WSProperties, float>(cond.Key) || (float)db.GetValue<WSProperties, float>(cond.Key) != cond.Value)
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
            if (!db.ContainsData<WSProperties, object>(cond.Key) || db.GetValue<WSProperties, object>(cond.Key) != cond.Value)
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
            foreach (WSProperties prop in Enum.GetValues(typeof(WSProperties)))
            {
                if (ws.db.ContainsData<WSProperties, Enum>(prop))
                {
                    Add(prop, ws.GetObj(prop));
                }
            }
        }
    }
}

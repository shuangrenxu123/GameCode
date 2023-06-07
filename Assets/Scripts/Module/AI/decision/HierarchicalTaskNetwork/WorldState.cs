using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace HTN
{
    /// <summary>
    /// �����ĵ�����״̬��Ϣ��
    /// </summary>
    public enum WSProperties
    {
        /// <summary>
        /// ����λ�ã�Enum��
        /// </summary>
        WS_Location,
        /// <summary>
        /// �Ƿ���Կ������ˣ�Bool��
        /// </summary>
        WS_CanSeeEnemy,
        /// <summary>
        /// ���ɣ��������;öȣ�Int��
        /// </summary>
        WS_TrunkHealth,
        /// <summary>
        /// ����Ƿ񿴼������ˣ�Bool��
        /// </summary>
        WS_HasSeenEnemyRecently,
        /// <summary>
        /// �����Ƿ񹥻�����Bool��
        /// </summary>
        WS_AttackedRecently,
        /// <summary>
        /// �Ƿ��Ѱ·������λ�ã�Bool��
        /// </summary>
        WS_CanNavigateToEnemy,
    }
    public enum Location
    {
        /// <summary>
        /// �ڵ��˸���
        /// </summary>
        EnemyLocRef,
        /// <summary>
        /// �Ÿ���
        /// </summary>
        NextBridgeLocRef,
        /// <summary>
        /// ���ɸ���
        /// </summary>
        FoundTrunk,
        /// <summary>
        /// ���һ�ε��˵�λ��
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
        /// ����״̬�Ļ���
        /// </summary>
        private Dictionary<WSProperties, Enum> wsEnum;
        private Dictionary<WSProperties, int> wsInt;
        private Dictionary<WSProperties, float> wsFloat;
        private Dictionary<WSProperties, bool> wsBool;
        private Dictionary<WSProperties, object> wsObj;

        /// <summary>
        /// ��������
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
        #region Add:�������״̬����
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
        #region Get:�������״̬����
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
        #region Set:��������״̬����
        /// <summary>
        /// ��������״̬����
        /// </summary>
        /// <param name="propType">����ö��</param>
        /// <param name="value">����ֵ</param>
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
        /// ��������״̬����
        /// </summary>
        /// <param name="propType">����ö��</param>
        /// <param name="value">����ֵ</param>
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
        /// ��������״̬����
        /// </summary>
        /// <param name="propType">����ö��</param>
        /// <param name="value">����ֵ</param>
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
        /// ��������״̬����
        /// </summary>
        /// <param name="propType">����ö��</param>
        /// <param name="value">����ֵ</param>
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
        /// ��������״̬����
        /// </summary>
        /// <param name="propType">����ö��</param>
        /// <param name="value">����ֵ</param>
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
        #region Contains���Ƿ����ĳЩ����
        /// <summary>
        /// �Ƿ����ĳЩ����
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
        /// �Ƿ����ĳЩ����
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
        /// �Ƿ����ĳЩ����
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
        /// �Ƿ����ĳЩ����
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
        /// �Ƿ����ĳЩ����
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
        /// ����һ������״̬�ĸ���
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
        /// ��ӡ����״̬������ʹ�ã�
        /// </summary>
        public void LogWorldState(string title)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in wsEnum)
            {
                sb.Append("//�������ԣ�").Append(item.Key.ToString()).Append("--����ֵ��").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsInt)
            {
                sb.Append("//�������ԣ�").Append(item.Key.ToString()).Append("--����ֵ��").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsBool)
            {
                sb.Append("//�������ԣ�").Append(item.Key.ToString()).Append("--����ֵ��").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsFloat)
            {
                sb.Append("//�������ԣ�").Append(item.Key.ToString()).Append("--����ֵ��").Append(item.Value.ToString()).Append("//\n");
            }
            foreach (var item in wsObj)
            {
                sb.Append("//�������ԣ�").Append(item.Key.ToString()).Append("--����ֵ��").Append(item.Value.ToString()).Append("//\n");
            }
            Debug.LogWarning(title + "  LogWorldState��\n" + sb.ToString());
        }
    }
}

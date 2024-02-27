using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class DefaultNetworkPackageCoder : NetWorkpackCoder
    {

        /// <summary>
        /// 包裹大小的字段类型
        /// 包裹有包头和包体组成
        /// </summary>
        public enum EpackageSizeFieldType
        {
            /// <summary>
            /// 包裹最大大约64kb
            /// </summary>
            UShort = 2,
            /// <summary>
            /// 包裹最大约2g
            /// </summary>
            Int = 4,
        }

        /// <summary>
        /// 消息ID的字段类型,可以理解为消息的id
        /// </summary>
        public enum EMessageIDFieldType
        {
            /// <summary>
            /// 取值范围：0 ～ 65535
            /// </summary>
            UShort = 2,
            /// <summary>
            /// 取值范围 -2147483648 ～ 2147483647 
            /// </summary>
            Int = 4,
        }

        public EpackageSizeFieldType PackageSizeFieldType = EpackageSizeFieldType.UShort;

        public EMessageIDFieldType MessageIDFieldType = EMessageIDFieldType.UShort;
        public override int GetPackageHeadSize()
        {
            int size = 0;
            size += (int)PackageSizeFieldType;
            size += (int)MessageIDFieldType;

            return size;
        }
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="sendBuffer">发送数组</param>
        /// <param name="packageObj">要发送的对象</param>
        public override void EnCode(ByteBuffer sendBuffer, object packageObj)
        {
            DefaultNetWorkPackage package = packageObj as DefaultNetWorkPackage;
            if (package == null)
            {
                Debug.LogError("发送的包不合逻辑");
                return;
            }
            if (package.Msgobj == null)
            {
                Debug.LogError("消息为null");
                return;
            }
            //获得包体数据
            byte[] bytes;
            bytes = _packageCoder.EnCode(package.Msgobj);
            if (bytes.Length > PackageBodyMaxSize)
            {
                return;
            }
            //包长，这里只是计算，并未写入
            //由于bytebuff底层将string的长度定位为ushort。所以这里只用加两个字节即可
            int packagerSize = (ushort)MessageIDFieldType + bytes.Length + 2;
            //写入包长
            if (PackageSizeFieldType == EpackageSizeFieldType.UShort)
            {
                sendBuffer.WriteUshort((ushort)packagerSize);
            }
            else
            {
                sendBuffer.WriteInt(packagerSize);
            }
            //写入包头
            if (MessageIDFieldType == EMessageIDFieldType.UShort)
            {
                if (package.MsgId > ushort.MaxValue)
                {
                    Debug.LogError("消息id超出");
                    return;
                }
                sendBuffer.WriteUshort((ushort)package.MsgId);
            }
            else
            {
                sendBuffer.WriteInt(package.MsgId);
            }
            sendBuffer.WriteString(package.SenderId);
            sendBuffer.WriteBytes(bytes, 0, bytes.Length);

        }
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="receiveBuffer">接收到的缓冲数组</param>
        /// <param name="outputList">目的地的列表</param>
        public override void Decode(ByteBuffer receiveBuffer, List<object> outputList)
        {
            while (true)
            {
                //剩余的未读取数据如果少于包头，也就是剩下的数据凑不够一个包
                //(int)PackageSizeFieldType = 4字节
                if (receiveBuffer.ReadableBytes() < (int)PackageSizeFieldType)
                    break;
                int packageSize;
                if (PackageSizeFieldType == EpackageSizeFieldType.UShort)
                    packageSize = receiveBuffer.ReadUshort();
                else
                    packageSize = receiveBuffer.ReadInt();

                if (receiveBuffer.ReadableBytes() < packageSize)
                {
                    break;//退出然后读够了数据再解包
                }
                DefaultNetWorkPackage packager = new DefaultNetWorkPackage();

                if (MessageIDFieldType == EMessageIDFieldType.UShort)
                {
                    packager.MsgId = receiveBuffer.ReadUshort();
                }
                else
                {
                    packager.MsgId = receiveBuffer.ReadInt();
                }
                //读取发送者
                packager.SenderId = receiveBuffer.ReadString();

                int bodySize = packageSize - (int)MessageIDFieldType - 2;
                if (bodySize > PackageBodyMaxSize)
                {
                    Debug.LogError("包太长了");
                    break;
                }
                try
                {
                    byte[] body = receiveBuffer.ReadBytes(bodySize);
                    Type classType = NetworkMessageRegister.TryGetMessageType(packager.MsgId);

                    if (classType != null)
                    {
                        packager.Msgobj = _packageCoder.DeCode(classType, body);
                        if (packager.Msgobj != null)
                        {
                            outputList.Add(packager);
                        }
                        else
                        {
                            Debug.LogError("解码失败");
                        }
                    }
                    else
                    {
                        Debug.LogError("没有注册相关类型");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            receiveBuffer.DiscardReadBytes();
        }

        protected byte[] EnCodeInternal(object obj)
        {
            //string json = JsonMapper.ToJson(obj);
            //byte[] data = Encoding.UTF8.GetBytes(json);

            byte[] data = (obj as IMessage).ToByteArray();
            //IMessage me = new PlayerInfo.login();
            //var o= me.Descriptor.Parser.ParseFrom(data) as PlayerInfo.login;
            return data;
        }
        protected object DeCodeInternal(Type type, byte[] body)
        {
            //string value = Encoding.UTF8.GetString(body);
            //object data = JsonMapper.ToObject(value);
            IMessage mess = (IMessage)Activator.CreateInstance(type);
            var data = mess.Descriptor.Parser.ParseFrom(body);
            return data;
        }
    }
}
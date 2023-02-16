using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace NetWork
{
    public class NetWorkManager : ModuleSingleton<NetWorkManager>, IModule
    {

        //实例化管理器的时候的参数
        public class CreateParameters
        {
            public Type PackageCoderType;

            public int PackageMaxSize = ushort.MaxValue;

        }
        private TcpClient client;
        //ip
        private string host;
        //端口
        private int port;

        /// <summary>
        /// 当前的连接状态
        /// </summary>
        public ENetWorkState state { private set; get; } = ENetWorkState.Disconnect;

        public NetworkMessageRegister register;
        /// <summary>
        /// Mono层的回调,用于处理包，每一帧都会处理接收到的包
        /// </summary>
        public PackageEvent handle;

        public void OnCreate(object createParam)
        {
            CreateParameters c = createParam as CreateParameters;
            if (createParam == null)
                Debug.LogError("传入的参数不对");
            client = new TcpClient(c.PackageCoderType, c.PackageMaxSize);
            handle = new();
            register = new NetworkMessageRegister();
            register.Init();
        }
        public void OnUpdate()
        {
            if (client == null)
                return;
            //更新客户端
            client.Update();

            while (true)
            {
                //循环处理所有接收到的网络包，已经解码好的
                DefaultNetWorkPackage package = (DefaultNetWorkPackage)client.PickPackage();
                //如果没有包了就退出循环
                if (package == null)
                {

                    return;
                }
                else if(handle != null)
                {
                    Debug.Log("接收到了消息");
                    handle?.Invoke(package);
                }

                //有可能socket突然断了
                if (state == ENetWorkState.Connected)
                {
                    if (!client.IsConnected())
                    {
                        state = ENetWorkState.Disconnect;
                        ///发送事件通知下说我断开了，还没写；
                        CloseClient();

                    }
                    return;
                }
            }
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void ConnectServer(string ip, int port)
        {
            if (state == ENetWorkState.Disconnect)
            {
                state = ENetWorkState.Connecting;

                IPEndPoint remote = new IPEndPoint(IPAddress.Parse(ip), port);
                client.ConnectAsync(remote, OnConnectServer);
            }
        }

        /// <summary>
        /// 异步连接成功后的回调函数
        /// </summary>
        /// <param name="obj"></param>
        private void OnConnectServer(SocketError obj)
        {
            if (obj == SocketError.Success)
            {
                state = ENetWorkState.Connected;
                Debug.Log("连接服务器成功");
                //这里应该会广播一个连接好的事件，负责通知说我连上了
            }
            else
            {
                state = ENetWorkState.Disconnect;
                //这里也是
                Debug.LogError("连接服务器失败");
            }
        }
        /// <summary>
        /// 自己主动关闭连接
        /// </summary>
        public void DisConnectServer()
        {
            if (state == ENetWorkState.Connected)
            {
                state = ENetWorkState.Disconnect;
                //发送广播，但我没写
                CloseClient();
                Debug.Log("与服务器的连接成功断开");
            }
        }
        /// <summary>
        /// 发送网络消息
        /// </summary>
        /// <param name="package"></param>
        public void SendMessage(int Msgid, object package)
        {
            if (state != ENetWorkState.Connected)
            {
                Debug.LogError("服务器未连接");
                return;
            }
            if (client != null)
            {
                var t = new DefaultNetWorkPackage();
                t.MsgId = Msgid;
                t.Msgobj = package;
                client.SendPackage(t);
            }
        }
        private void CloseClient()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }

 public class PackageEvent : UnityEvent<DefaultNetWorkPackage>
    {

    } 
}
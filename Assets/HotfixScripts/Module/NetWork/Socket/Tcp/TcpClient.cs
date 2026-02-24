using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Network;
using ObjectPool;
using UnityEngine;
using UnityEngine.Events;
namespace Network.Tcp
{
    /// <summary>
    /// Tcp客户端
    /// </summary>
    public class TcpClient : IDisposable
    {
        private class PackageEvent : UnityEvent<DefaultNetWorkPackage>
        {
        }

        private class UserToken
        {
            public System.Action<SocketError> Callback;
        }
        private static readonly object messageRegisterLock = new object();
        private static bool messageRegisterInitialized;
        private readonly Type packageCoderType;//单个逻辑包类型
        private readonly Type packageBodyCoderType;//数据解码器类型
        private readonly int packageBodyMaxSize;
        /// <summary>
        /// 具体的socket
        /// </summary>
        private TcpChannel channel;
        private readonly Dictionary<int, PackageEvent> handles = new Dictionary<int, PackageEvent>();

        private MainThreadSyncContext context;
        /// <summary>
        /// 当前的连接状态
        /// </summary>
        public ENetWorkState state { private set; get; } = ENetWorkState.Disconnect;
        public TcpClient(Type packageType, Type bodyType, int size)
        {
            packageCoderType = packageType;
            packageBodyCoderType = bodyType;
            packageBodyMaxSize = size;
            context = new MainThreadSyncContext();
            EnsureMessageRegisterInitialized();
        }

        private void Init(SocketAsyncEventArgs e)
        {
            channel = new TcpChannel();
            if (e == null)
            {
                channel.Init(context, null, packageCoderType, packageBodyCoderType, packageBodyMaxSize);
            }
            else
            {
                channel.Init(context, e.ConnectSocket, packageCoderType, packageBodyCoderType, packageBodyMaxSize);
            }
        }
        public void Dispose()
        {
            if (channel != null)
            {
                channel.Dispose();
                channel = null;
            }
            state = ENetWorkState.Disconnect;
        }

        public void Update()
        {
            if (channel != null)
                channel.Update();
            if (context != null)
            {
                context.Update();
            }
            ProcessPackages();
        }
        public void SendPackage(object obj)
        {
            if (channel != null)
                channel.SendPackage(obj);
            else
                Debug.LogError("channel is null");


        }
        public void RegisterHandle(int messageId, UnityAction<DefaultNetWorkPackage> action)
        {
            if (handles.TryGetValue(messageId, out var handle))
            {
                handle.AddListener(action);
            }
            else
            {
                var temp = new PackageEvent();
                temp.AddListener(action);
                handles.Add(messageId, temp);
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
                ConnectAsync(remote, OnConnectServer);
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
        public void SendMessage(string senderid, int msgid, object package)
        {
            if (state != ENetWorkState.Connected)
            {
                Debug.LogError("服务器未连接");
                return;
            }

            var t = ReferenceManager.Instance.Spawn<DefaultNetWorkPackage>();
            t.SenderId = senderid;
            t.MsgId = msgid;
            t.MsgObj = package;

            SendPackage(t);
        }
        /// <summary>
        /// 获取一个包
        /// </summary>
        /// <returns></returns>
        public object PickPackage()
        {
            if (channel != null)
                return channel.PickPackage();
            else
                return null;
        }
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (channel == null)
                return false;
            else
                return channel.IsConnected();
        }
        /// <summary>
        /// 尝试异步获取连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="callback"></param>
        public void ConnectAsync(IPEndPoint ip, Action<SocketError> callback)
        {
            UserToken token = new UserToken
            {
                Callback = callback,
            };
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = ip;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArgs_Completed);
            args.UserToken = token;

            Socket clientSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            bool willRaiseEvent = clientSocket.ConnectAsync(args);
            if (!willRaiseEvent)
            {
                ProcessConnected(args);
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
                // EventManager.Instance.SendMessage("ConnectServerSuccess", null);
            }
            else
            {
                state = ENetWorkState.Disconnect;
                Debug.LogError("连接服务器失败");
                // EventManager.Instance.SendMessage("ConnectServerFaild", null);
            }
        }
        /// <summary>
        /// 处理连接信息
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessConnected(object obj)
        {
            SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
            UserToken token = e.UserToken as UserToken;
            if (e.SocketError == SocketError.Success)
            {
                if (channel != null)
                    throw new Exception("TcpChannel 已经创建了");
                Init(e);
            }
            else
            {
                Debug.LogError(e.SocketError);
            }
            //执行玩家设定的回调函数
            if (token.Callback != null)
            {
                token.Callback.Invoke(e.SocketError);
            }
        }

        private void CloseClient()
        {
            Dispose();
        }

        private void ProcessPackages()
        {
            while (true)
            {
                //循环处理所有接收到的网络包，已经解码好的
                DefaultNetWorkPackage package = (DefaultNetWorkPackage)PickPackage();
                //如果没有包了就退出循环
                if (package == null)
                {
                    break;
                }

                if (handles.TryGetValue(package.MsgId, out var handle) && handle != null)
                {
                    handle.Invoke(package);
                }
                else
                {
                    Debug.LogWarning($"未注册的消息Id: {package.MsgId}");
                }

                ReferenceManager.Instance.Release(package);
            }

            //有可能socket突然断了
            if (state == ENetWorkState.Connected && !IsConnected())
            {
                state = ENetWorkState.Disconnect;
                ///发送事件通知下说我断开了，还没写；
                CloseClient();
            }
        }

        private static void EnsureMessageRegisterInitialized()
        {
            if (messageRegisterInitialized)
                return;

            lock (messageRegisterLock)
            {
                if (messageRegisterInitialized)
                    return;

                var register = new NetworkMessageRegister();
                register.Init();
                messageRegisterInitialized = true;
            }
        }
        /// <summary>
        /// 回归主线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    context.Post(ProcessConnected, e);
                    break;
                default:
                    break;
            }
        }
    }
}

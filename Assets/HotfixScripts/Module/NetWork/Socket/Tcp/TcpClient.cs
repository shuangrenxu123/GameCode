using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
namespace Network
{
    /// <summary>
    /// Tcp客户端
    /// </summary>
    public class TcpClient : IDisposable
    {
        private class UserToken
        {
            public System.Action<SocketError> Callback;
        }
        private readonly Type packageCoderType;//单个逻辑包类型
        private readonly Type packageBodyCoderType;//数据解码器类型
        private readonly int packagebodyMaxSize;
        /// <summary>
        /// 具体的socket
        /// </summary>
        private TcpChannel channel;

        private MainThreadSyncContext context;
        public TcpClient(Type packagetype,Type bodyType, int size)
        {
            packageCoderType = packagetype;
            packageBodyCoderType = bodyType;
            packagebodyMaxSize = size;
            context = new MainThreadSyncContext();
        }

        private void Init(SocketAsyncEventArgs e)
        {
            channel = new TcpChannel();
            if (e == null)
            {
                channel.Init(context, null, packageCoderType, packageBodyCoderType, packagebodyMaxSize);
            }
            else
            {
                channel.Init(context, e.ConnectSocket, packageCoderType, packageBodyCoderType,packagebodyMaxSize);
            }
        }
        public void Dispose()
        {
            if (channel != null)
            {
                channel.Dispose();
                channel = null;
            }
        }

        public void Update()
        {
            if (channel != null)
                channel.Update();
            if (context != null)
            {
                context.Update();
            }
        }
        public void SendPackage(object obj)
        {
            if (channel != null)
                channel.SendPackage(obj);
            else
                Debug.LogError("channel is null");


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
            args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArgs_Comletep);
            args.UserToken = token;

            Socket clientSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            bool willRaiseEvent = clientSocket.ConnectAsync(args);
            if (!willRaiseEvent)
            {
                ProcessConnected(args);
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
        /// <summary>
        /// 回归主线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptEventArgs_Comletep(object sender, SocketAsyncEventArgs e)
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
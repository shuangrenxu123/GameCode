using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NetWork
{
    public class TcpChannel : IDisposable
    {
        private SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        private NetWorkpackCoder packageCoder;

        private readonly Queue<System.Object> sendQueue = new Queue<object>();
        private readonly Queue<System.Object> receiveQueue = new Queue<object>();
        private readonly List<System.Object> decodeTempList = new List<object>();

        private Socket socket;//通讯用的socket
        private int packageMaxSize;//最大包长
        private byte[] receiveBuffer;//接收的数组缓冲区 

        private ByteBuffer sendBuffer;
        private ByteBuffer decodeBuffer;

        private bool isSending = false;
        private bool isReceive = false;

        private MainThreadSyncContext context;

        /// <summary>
        /// 初始化Channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sock">与服务端链接的socket</param>
        /// <param name="type">编码器类型</param>
        /// <param name="Bodysize">包长</param>
        /// <exception cref="System.ArgumentException"></exception>
        public void Init(MainThreadSyncContext context, Socket sock, Type type, int Bodysize)
        {
            if (type == null)
                throw new System.ArgumentException($"packageCoderType is null.");
            if (Bodysize <= 0)
                throw new System.ArgumentException($"packageMaxSize is invalid : {Bodysize}");

            socket = sock;
            packageMaxSize = Bodysize;
            //创建编码解码器
            packageCoder = (NetWorkpackCoder)Activator.CreateInstance(type);
            //初始化编码解码
            packageCoder.Init(this, Bodysize);
            //最大的尺寸等于 包体加包头
            packageMaxSize = Bodysize + packageCoder.GetPackageHeadSize();
            this.context = context;
            //创建字节缓冲类，推荐4倍最大包体长度
            int byteBufferSize = packageMaxSize * 4;
            int tempBufferSize = packageMaxSize * 2;
            sendBuffer = new ByteBuffer(byteBufferSize);
            decodeBuffer = new ByteBuffer(byteBufferSize);
            receiveBuffer = new byte[tempBufferSize];

            //创建IOCP接收类
            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Comleted);
            receiveArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);

            //创建IOCP发送类
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Comleted);
            sendArgs.SetBuffer(sendBuffer.GetBuffer(), 0, sendBuffer.Capacity);

        }

        /// <summary>
        /// 是否连接中
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (socket != null)
                return socket.Connected;
            else
                return false;
        }
        public void Dispose()
        {
            try
            {
                if (socket != null)
                    socket.Shutdown(SocketShutdown.Both);
                receiveArgs.Dispose();
                sendArgs.Dispose();

                sendQueue.Clear();
                receiveQueue.Clear();
                decodeTempList.Clear();

                sendBuffer.Clear();
                decodeBuffer.Clear();

                isSending = false;
                isReceive = false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void Update()
        {
            if (socket == null || socket.Connected == false)
                return;
            UpdateReceiving();
            UpdateSending();
        }
        private void UpdateSending()
        {
            if (sendQueue.Count > 0)
            {
                //清空缓存
                sendBuffer.Clear();
                //合并数据一起发送
                while (sendQueue.Count > 0 && !isSending)
                {
                    isSending = true;
                    if (sendBuffer.WriteBytes() < packageMaxSize)
                        break;

                    object packet = sendQueue.Dequeue();//取出一个对象
                    packageCoder.EnCode(sendBuffer, packet);//编码并添加到发送数组中
                    sendArgs.SetBuffer(0, sendBuffer.ReadableBytes());//设置缓冲区，0为开始处的位置，后面的参数为可接受的最大数据量
                    bool willRaiseEvent = socket.SendAsync(sendArgs);
                    if (!willRaiseEvent)
                        ProcessSend(sendArgs);
                }
            }
        }
        private void UpdateReceiving()
        {
            if (!isReceive)
            {
                isReceive = true;
                bool WillRaiseEvent = socket.ReceiveAsync(receiveArgs);
                if (!WillRaiseEvent)
                    ProcessReceive(receiveArgs);

            }
        }
        /// <summary>
        /// 将一个类加入到待发送队列
        /// </summary>
        /// <param name="packet"></param>
        public void SendPackage(object packet)
        {
            lock (sendQueue)
            {
                sendQueue.Enqueue(packet);
            }
        }
        /// <summary>
        /// 获取一个网络包
        /// </summary>
        public object PickPackage()
        {
            object package = null;
            lock (receiveQueue)
            {
                if (receiveQueue.Count > 0)
                {
                    package = receiveQueue.Dequeue();
                }
            }
            return package;
        }
        /// <summary>
        /// 消息接收完成时
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessReceive(object obj)
        {
            SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
            //socket传输的字节数
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                UnityEngine.Debug.Log(e.BytesTransferred);
                //判断能否写入待解码缓冲区
                if (!decodeBuffer.CanWriteable(e.BytesTransferred))
                {
                    return;
                }
                decodeBuffer.WriteBytes(e.Buffer, 0, e.BytesTransferred);//将socket中收到的数据传入到待解码的缓冲区
                decodeTempList.Clear();//清空临时缓冲列表
                packageCoder.Decode(decodeBuffer, decodeTempList);//将接受到的数据解码并存到解码临时列表中
                lock (receiveArgs)//接收到的信息
                {
                    for (int i = 0; i < decodeTempList.Count; i++)
                    {
                        receiveQueue.Enqueue(decodeTempList[i]);
                    }
                }
                //解码完成后等待接收下次的数据
                e.SetBuffer(0, receiveBuffer.Length);
                isReceive = false;
            }
        }
        /// <summary>
        /// 消息发送完成时
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessSend(object obj)
        {
            SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
            //success 正常
            if (e.SocketError == SocketError.Success)
            {
                isSending = false;
            }
            else
            {
                UnityEngine.Debug.LogError("发送信息失败");
            }
        }
        /// <summary>
        /// 每当执行完发送或者接收的时候会调用这个方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void IO_Comleted(object sender, SocketAsyncEventArgs s)
        {
            //上个操作的类型
            switch (s.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    context.Post(ProcessReceive, s);
                    break;
                case SocketAsyncOperation.Send:
                    context.Post(ProcessSend, s);
                    break;
                default:
                    break;
            }
        }
    }
}
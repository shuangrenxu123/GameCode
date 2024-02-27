using System;
using System.Collections.Generic;

namespace Network
{
    public abstract class NetWorkpackCoder
    {
        public int PackageBodyMaxSize;

        protected TcpChannel tcpChannel;
        protected IPackageCoder _packageCoder;
        public void Init(TcpChannel channel, int size,Type coderType)
        {
            tcpChannel = channel;
            PackageBodyMaxSize = size;
            _packageCoder = (IPackageCoder)Activator.CreateInstance(coderType);
        }
        /// <summary>
        /// 获得包头长度
        /// </summary>
        /// <returns></returns>
        public abstract int GetPackageHeadSize();
        /// <summary>
        /// 编码
        /// </summary>
        public abstract void EnCode(ByteBuffer sendBuffer, object packageObj);
        /// <summary>
        /// 解码
        /// </summary>
        public abstract void Decode(ByteBuffer receiveBuffer, List<object> outputList);
    }
}

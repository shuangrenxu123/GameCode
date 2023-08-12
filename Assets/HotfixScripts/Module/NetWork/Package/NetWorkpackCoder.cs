using System.Collections.Generic;

namespace NetWork
{
    public abstract class NetWorkpackCoder
    {
        private TcpChannel tcpChannel;
        public int PackageBodyMaxSize;
        public void Init(TcpChannel channel, int size)
        {
            tcpChannel = channel;
            PackageBodyMaxSize = size;
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

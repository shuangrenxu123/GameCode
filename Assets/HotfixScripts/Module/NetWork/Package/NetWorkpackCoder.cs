using System;
using System.Collections.Generic;
using Network.Tcp;

namespace Network
{
    public abstract class NetWorkPackCoder
    {
        public int PackageBodyMaxSize;

        protected IPackageCoder _packageCoder;
        public void Init(int size, Type coderType)
        {
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

using System;

namespace Network
{
    public interface IPackageCoder
    {

        public byte[] EnCode(object package);
        public object DeCode(Type type, byte[] bytes);
    }
}
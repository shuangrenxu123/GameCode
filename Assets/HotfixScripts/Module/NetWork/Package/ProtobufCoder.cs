using Google.Protobuf;
using System;

namespace Network
{
    public class ProtobufCoder : IPackageCoder
    {
        public object DeCode(Type type, byte[] bytes)
        {
            //var package = ReferenceManager.Instance.Spawn(type);
            IMessage mess = (IMessage)Activator.CreateInstance(type);
            //IMessage mess = (IMessage)package;
            var data = mess.Descriptor.Parser.ParseFrom(bytes);
            return data;
        }

        public byte[] EnCode(object package)
        {
            byte[] data = (package as IMessage).ToByteArray();
            return data;
        }
    }
}

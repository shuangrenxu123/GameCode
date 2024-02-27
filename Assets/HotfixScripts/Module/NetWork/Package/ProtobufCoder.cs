using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network {
    public class ProtobufCoder : IPackageCoder
    {
        public object DeCode(Type type, byte[] bytes)
        {
            IMessage mess = (IMessage)Activator.CreateInstance(type);
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

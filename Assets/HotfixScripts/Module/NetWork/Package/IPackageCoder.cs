using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IPackageCoder
    {

        public byte[] EnCode(object package);
        public object DeCode(Type type, byte[] bytes);
    }
}
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utilities.Collections
{
    [StructLayout(LayoutKind.Explicit)]
    public class Union<A, B>
    {
        [FieldOffset(0)]
        public bool isA;
        [FieldOffset(2)] public A a;
        [FieldOffset(2)] public B b;
        public Union(A a)
        {
            this.a = a;
            b = default;
            isA = true;
        }

        public Union(B b)
        {
            this.a = default;
            this.b = b;
            isA = false;
        }
    }
}

using System;
using UnityEngine;
namespace Helper
{
    public class RuntimeException : Exception
    {
        public Token token;
        public RuntimeException(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}

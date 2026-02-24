using System;
using System.Collections.Generic;
using Network.Tcp;
using UnityEngine;

namespace Network
{
    public class NetWorkManager : ModuleSingleton<NetWorkManager>, IModule
    {
        //实例化管理器的时候的参数
        public class CreateParameters
        {
            public Type PackageCoderType;
            public Type PackageBodyCoderType;
            public int PackageMaxSize = ushort.MaxValue;
        }

        private List<TcpClient> clients;
        public void OnCreate(object createParam)
        {
            clients = new();
        }
        public void OnUpdate()
        {
            if (clients == null)
            {
                return;
            }

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                clients[i]?.Update();
            }
        }

        public void DestroyClient(TcpClient client)
        {
            if (clients == null)
                return;

            client.Dispose();
            clients.Remove(client);
        }

        public TcpClient CreateTcpClient(CreateParameters param)
        {
            if (clients == null)
            {
                clients = new List<TcpClient>();
            }
            if (param == null)
            {
                Debug.LogError("TcpClient 参数未初始化");
                return null;
            }
            if (param.PackageCoderType == null || param.PackageBodyCoderType == null)
            {
                Debug.LogError("TcpClient 参数未初始化");
                return null;
            }
            var client = new TcpClient(param.PackageCoderType, param.PackageBodyCoderType, param.PackageMaxSize);
            clients.Add(client);
            return client;
        }

    }
}

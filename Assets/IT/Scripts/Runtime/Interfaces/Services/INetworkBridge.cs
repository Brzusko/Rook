using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;

namespace IT.Interfaces
{
    public interface INetworkBridge : IServiceWithDependency
    {
        public string Address { get; set; }
        public ushort Port { get; set; }
        public bool IsClientCreated { get; }
        public bool IsServerCreated { get; }
        public int MaxClients { get; set; }
        public bool ShouldAcceptConnections { get; set; }
        public int ConnectedClients { get; }

        public void StartServer();
        public void StartClient();
        public void StopServer();
        public void StopClient();
    }
}

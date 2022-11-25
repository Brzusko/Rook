using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using FishNet.Managing.Transporting;
using FishNet.Transporting;
using IT.Interfaces;
using UnityEngine;

namespace IT
{
    public class SimpleNetworkBridge : Service, INetworkBridge
    {
        [SerializeField] private ServerManager _serverManager;
        [SerializeField] private ClientManager _clientManager;
        [SerializeField] private TransportManager _transportManager;
        private Transport _transport;
        
        public override Type Type => typeof(INetworkBridge);
        public string Address { get; set; }
        public ushort Port { get; set; }
        public bool IsClientCreated => _clientManager.Started;
        public bool IsServerCreated => _serverManager.Started;
        
        public void StartServer()
        {
            throw new NotImplementedException();
        }

        public void StartClient()
        {
            throw new NotImplementedException();
        }

        public void StopServer()
        {
            throw new NotImplementedException();
        }

        public void StopClient()
        {
            throw new NotImplementedException();
        }
    }
}
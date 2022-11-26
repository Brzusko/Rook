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
        private bool _isRegistered;
        
        public override Type Type => typeof(INetworkBridge);
        
        
        public string Address
        {
            get => _transport.GetClientAddress();
            set
            {
                if(IsServerCreated || IsClientCreated)
                    return;
                
                _transport.SetClientAddress(value);
            }
        }

        public ushort Port
        {
            get => _transport.GetPort();
            set
            {
                if(IsServerCreated || IsClientCreated)
                    return;
                
                _transport.SetPort(value);
            }
        }

        public int MaxClients
        {
            get => _transport.GetMaximumClients();
            set
            {
                if(IsServerCreated || IsClientCreated)
                    return;
                
                _transport.SetMaximumClients(value);
            }
        }
        public bool IsClientCreated => _clientManager.Started;
        public bool IsServerCreated => _serverManager.Started;

        private void Start()
        {
            _transport = _transportManager.Transport;
            RegisterToContainer();
        }

        private void OnDestroy()
        {
            UnregisterFromContainer();
        }

        public void StartServer()
        {
            if(IsServerCreated)
                return;
            
            _serverManager.StartConnection();
        }

        public void StartClient()
        {
            if(IsClientCreated)
                return;
            
            _clientManager.StartConnection();
        }

        public void StopServer()
        {
            if(!IsServerCreated)
                return;

            _serverManager.StopConnection(true);
        }

        public void StopClient()
        {
            if(!IsClientCreated)
                return;
            
            _clientManager.StopConnection();
        }
        
        private void RegisterToContainer()
        {
            if(_isRegistered || ServiceContainer.Contains<INetworkBridge>())
                return;
            
            ServiceContainer.RegisterService<INetworkBridge>(this);
            _isRegistered = true;
        }

        private void UnregisterFromContainer()
        {
            if(!_isRegistered)
                return;
            
            ServiceContainer.UnregisterService<INetworkBridge>();
        }
    }
}
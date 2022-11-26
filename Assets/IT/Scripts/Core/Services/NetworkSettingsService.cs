using System;
using System.Collections;
using System.Collections.Generic;
using IT.Data;
using IT.Interfaces;
using UnityEngine;

namespace IT
{
    public class NetworkSettingsService : MonoBehaviour, INetworkSettingsService
    {
        public Type Type => typeof(INetworkSettingsService);
        public GameObject GameObject => gameObject;

        private bool _isRegistered = false;
        private NetworkSettings _data;
        private IFileProcessor<NetworkSettings> _fileProcessor;

        public ushort Port
        {
            get => _data.Port;
            set
            {
                _data.Port = value;
                SaveContent();
            }
        }

        public string Address
        {
            get => _data.Address;
            set
            {
                _data.Address = value;
                SaveContent();
            }
        }

        public int MaxClients
        {
            get => _data.MaxClients;
            set
            {
                _data.MaxClients = value;
                SaveContent();
            }
        }

        public bool AsServer
        {
            get => _data.AsDedicatedServer;
            set
            {
                _data.AsDedicatedServer = value;
                SaveContent();
            }
        }
        
        private void Start()
        {
            RegisterToContainer();
            _fileProcessor = new NetworkSettingsYAMLProcessor($"{Application.persistentDataPath}/network-settings.yml");
            _data = _fileProcessor.GrabFileContent();
        }

        private void OnDestroy()
        {
            UnregisterFromContainer();
            _fileProcessor.Prune();
        }

        private void RegisterToContainer()
        {
            if(_isRegistered || ServiceContainer.Contains<INetworkSettingsService>())
                return;
            
            ServiceContainer.RegisterService<INetworkSettingsService>(this);
            _isRegistered = true;
        }

        private void UnregisterFromContainer()
        {
            if(!_isRegistered)
                return;
            
            ServiceContainer.UnregisterService<INetworkSettingsService>();
        }

        private void SaveContent()
        {
            _fileProcessor.WriteContent(_data.ToWriteData());
            _fileProcessor.SaveCache();
        }

    }
}

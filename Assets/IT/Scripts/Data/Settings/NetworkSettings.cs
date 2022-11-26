using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Data
{
    public class NetworkSettings : BaseSettings
    {
        public ushort Port { get; set; }
        public string Address { get; set; }
        public int MaxClients { get; set; }
        public bool AsDedicatedServer { get; set; }

        public NetworkSettings()
        {
            Port = 7171;
            Address = "127.0.0.1";
            MaxClients = 200;
            AsDedicatedServer = false;
            Version = 1;
        }
    }
}

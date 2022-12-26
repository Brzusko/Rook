using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using QFSW.QC;
using UnityEngine;

namespace IT.Commands
{
    public static class NetworkBridgeCommands
    {
        [Command("remote-address")]
        public static string RemoteAddress
        {
            get => ServiceContainer.Get<INetworkBridge>().Address;
            set
            {
                ServiceContainer.Get<INetworkSettingsService>().Address = value;
                ServiceContainer.Get<INetworkBridge>().Address = value;
            }
        }
        
        [Command("remote-port")]
        public static ushort RemotePort
        {
            get => ServiceContainer.Get<INetworkBridge>().Port;
            set
            {
                ServiceContainer.Get<INetworkSettingsService>().Port = value;
                ServiceContainer.Get<INetworkBridge>().Port = value;
            }
        }
        
        [Command("max-clients")]
        public static int MaxClients
        {
            get => ServiceContainer.Get<INetworkBridge>().MaxClients;
            set
            {
                ServiceContainer.Get<INetworkSettingsService>().MaxClients = value;
                ServiceContainer.Get<INetworkBridge>().MaxClients = value;
            }
        }

        [Command("is-dedicated-server")]
        public static bool AsDedicatedServer
        {
            get => ServiceContainer.Get<INetworkSettingsService>().AsServer;
            set => ServiceContainer.Get<INetworkSettingsService>().AsServer = value;
        }
        
        [Command("start-server")]
        public static void StartServer() => ServiceContainer.Get<INetworkBridge>().StartServer();
        
        [Command("stop-server")]
        public static void StopServer() => ServiceContainer.Get<INetworkBridge>().StopServer();
        
        [Command("start-client")]
        public static void StartClient() => ServiceContainer.Get<INetworkBridge>().StartClient();
        
        [Command("stop-client")]
        public static void StopClient() => ServiceContainer.Get<INetworkBridge>().StopClient();
        
        [Command("start-host")]
        public static void StartHost()
        {
            INetworkBridge _networkBridge = ServiceContainer.Get<INetworkBridge>();
            _networkBridge.StartServer();
            _networkBridge.StartClient();
        }
        
        [Command("stop-host")]
        public static void StopHost()
        {
            INetworkBridge _networkBridge = ServiceContainer.Get<INetworkBridge>();
            _networkBridge.StopServer();
            _networkBridge.StopClient();
        }
    }
}

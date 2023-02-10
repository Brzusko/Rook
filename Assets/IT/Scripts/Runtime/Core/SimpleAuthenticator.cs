using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Authenticating;
using FishNet.Connection;
using IT;
using IT.Interfaces;
using UnityEngine;

public class SimpleAuthenticator : Authenticator
{
    public override event Action<NetworkConnection, bool> OnAuthenticationResult;

    public override void OnRemoteConnection(NetworkConnection connection)
    {
        INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();
        OnAuthenticationResult?.Invoke(connection, networkBridge.ShouldAcceptConnections);
    }
}

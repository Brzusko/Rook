using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

namespace IT.Lobby
{
    public class LobbyWaiter
    {
        public Color WaiterColor { get; set; }
        public NetworkConnection Connection { get; set; }
        public bool IsReady { get; set; }
    }

    public static class LobbyWaiterExtenstion
    {
        public static LobbyWaiterSendData ToSendData(this LobbyWaiter waiter)
        {
            LobbyWaiterSendData sendData = default;
            sendData.WaiterColor = waiter.WaiterColor;
            sendData.IsReady = waiter.IsReady;
            sendData.WaiterID = waiter.Connection.ClientId;
            return sendData;
        }
    }
    
    public struct LobbyWaiterSendData
    {
        public Color WaiterColor;
        public int WaiterID;
        public bool IsReady;
    }
}

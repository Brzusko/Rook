using System;
using System.Collections;
using System.Collections.Generic;
using IT.Lobby;
using UnityEngine;

namespace IT.ScriptableObjects.UI
{
    [CreateAssetMenu(fileName = "Lobby Binding", menuName = "UI/Bindings/Lobby Binding")]
    public class LobbyBinding : ScriptableObject
    {
        // Other classes listen to
        public event Action ReadyClicked;
        public event Action ExitClicked;
        
        public void FireReadyClicked() => ReadyClicked?.Invoke();
        public void FireExitClicked() => ExitClicked?.Invoke();
        
        // UI Listen to
        public event Action<IEnumerable<LobbyWaiterSendData>> LobbyWaitersPropagation;
        public event Action<string> LobbyMessage;

        public void FireLobbyWaitersPropagation(IEnumerable<LobbyWaiterSendData> lobbyWaiterSendData) =>
            LobbyWaitersPropagation?.Invoke(lobbyWaiterSendData);

        public void FireLobbyMessage(string message) => LobbyMessage?.Invoke(message);
    }
}

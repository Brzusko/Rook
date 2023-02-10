using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IT.UI
{
    public class LobbyWaiter : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<LobbyWaiter, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription playerNameAttr = new UxmlStringAttributeDescription
            {
                name = "PlayerName"
            };

            private UxmlBoolAttributeDescription playerReadyAttr = new UxmlBoolAttributeDescription
            {
                name = "IsReady"
            };

            private UxmlColorAttributeDescription playerColorAttr = new UxmlColorAttributeDescription
            {
                name = "PlayerColor"
            };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);

                LobbyWaiter lobbyWaiter = visualElement as LobbyWaiter;
                lobbyWaiter.PlayerName = playerNameAttr.GetValueFromBag(bag, context);
                lobbyWaiter.IsReady = playerReadyAttr.GetValueFromBag(bag, context);
                lobbyWaiter.PlayerColor = playerColorAttr.GetValueFromBag(bag, context);
            }
        }

        private static readonly string _stylesheet = "LobbyWaiter";
        private static readonly string _mainContainerClass = "lobby_waiter_container";
        private static readonly string _lobbyWaiterColorClass = "lobby_waiter_player_color";
        private static readonly string _lobbyWaiterInfoContainerClass = "lobby_waiter_info_container";
        
        private Label _playerNameLabel;
        private Label _playerStateLabel;
        private VisualElement _playerColorVisualElement;
        
        private string _playerName;
        private bool _isReady;
        private Color _playerColor;

        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value;
                
                if(_playerNameLabel == null)
                    return;

                _playerNameLabel.text = _playerName ?? "Player";
            }
        }

        public bool IsReady
        {
            get => _isReady;
            set
            {
                _isReady = value;
                
                if(_playerStateLabel == null)
                    return;

                _playerStateLabel.text = _isReady ? "is ready" : "is not ready";
            }
        }

        public Color PlayerColor
        {
            get => _playerColor;
            set
            {
                _playerColor = value;
                
                if(_playerColorVisualElement == null)
                    return;

                _playerColorVisualElement.style.backgroundColor = _playerColor;
            }
        }

        public LobbyWaiter()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(_stylesheet));
            VisualElement mainContainer = new VisualElement();
            hierarchy.Add(mainContainer);
            mainContainer.AddToClassList(_mainContainerClass);

            VisualElement playerInfoContainer = new VisualElement();
            playerInfoContainer.AddToClassList(_lobbyWaiterInfoContainerClass);
            _playerColorVisualElement = new VisualElement();
            _playerColorVisualElement.AddToClassList(_lobbyWaiterColorClass);
            
            _playerNameLabel = new Label();
            
            mainContainer.Add(playerInfoContainer);
            
            playerInfoContainer.Add(_playerColorVisualElement);
            playerInfoContainer.Add(_playerNameLabel);

            _playerStateLabel = new Label();
            mainContainer.Add(_playerStateLabel);
        }
    }
}

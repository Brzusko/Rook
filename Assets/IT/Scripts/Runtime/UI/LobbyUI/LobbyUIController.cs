using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IT.Lobby;
using IT.ScriptableObjects.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace IT.UI
{
    public class LobbyUIController : Controller
    {
        private static readonly string _waiterListName = "WaiterList";
        private static readonly string _messageLabelName = "Message";
        private static readonly string _readyButtonName = "Ready";
        private static readonly string _exitButtonName = "Exit";

        [SerializeField] private LobbyBinding _lobbyBinding;

        private List<LobbyWaiterSendData> _waitersCache;

        public override ControllerIDs ControllerID => ControllerIDs.LOBBY;

        public override void Hide()
        {
            base.Hide();
            
            if(_waitersCache != null)
                CleanUp();
        }

        protected override void BindUI()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            Button readyButton = root.Q<Button>(_readyButtonName);
            Button exitButton = root.Q<Button>(_exitButtonName);

            _lobbyBinding.LobbyWaitersPropagation += OnLobbyWaitersPropagation;
            readyButton.clicked += _lobbyBinding.FireReadyClicked;
            exitButton.clicked += _lobbyBinding.FireExitClicked;
            _lobbyBinding.LobbyMessage += OnMessage;
        }

        protected override void UnbindUI()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            Button readyButton = root.Q<Button>(_readyButtonName);
            Button exitButton = root.Q<Button>(_exitButtonName);

            _lobbyBinding.LobbyWaitersPropagation -= OnLobbyWaitersPropagation;
            readyButton.clicked -= _lobbyBinding.FireReadyClicked;
            exitButton.clicked -= _lobbyBinding.FireExitClicked;
            _lobbyBinding.LobbyMessage -= OnMessage;
        }

        private void OnLobbyWaitersPropagation(IEnumerable<LobbyWaiterSendData> waitersSendData)
        {
            List<LobbyWaiterSendData> lobbyWaiterSendData = waitersSendData.ToList();
            VisualElement root = _uiDocument.rootVisualElement;
            VisualElement waiterList = root.Q<VisualElement>(_waiterListName);
            
            if (waiterList == null)
            {
                Debug.LogError("Cannot find waiter list");
                return;
            }
            
            if (_waitersCache == null)
            {
                foreach (LobbyWaiterSendData waiterSendData in lobbyWaiterSendData)
                {
                    CreateWaiter(waiterSendData, waiterList);
                }

                _waitersCache = lobbyWaiterSendData;
                
                return;
            }

            IEnumerable<LobbyWaiterSendData> waitersToRemove = _waitersCache.Where(waiter =>
                lobbyWaiterSendData.All(innerWaiter => !waiter.Equals(innerWaiter)));
            IEnumerable<LobbyWaiterSendData> waitersToCreate =
                lobbyWaiterSendData.Where(waiter => _waitersCache.All(innerWaiter => !waiter.Equals(innerWaiter)));

            RemoveMissings(waitersToRemove, waiterList);
            UpdateWaiters(lobbyWaiterSendData, waiterList);
            CreateNewWaiters(waitersToCreate, waiterList);

            _waitersCache = lobbyWaiterSendData;
        }

        private void OnMessage(string message)
        {
            VisualElement root = _uiDocument.rootVisualElement;
            Label messageLabel = root.Q<Label>(_messageLabelName);

            messageLabel.text = message;
        }

        private void RemoveMissings(IEnumerable<LobbyWaiterSendData> missings, VisualElement waiterList)
        {
            foreach (LobbyWaiterSendData waiter in missings)
            {
                LobbyWaiter waiterUI = waiterList.Q<LobbyWaiter>(waiter.WaiterID.ToString());
                
                if(waiterUI != null)
                    waiterList.Remove(waiterUI);
            }
        }
        
        private void UpdateWaiters(IEnumerable<LobbyWaiterSendData> toUpdate, VisualElement waiterList)
        {
            foreach (LobbyWaiterSendData waiter in toUpdate)
            {
                LobbyWaiter waiterUI = waiterList.Q<LobbyWaiter>(waiter.WaiterID.ToString());

                if (waiterUI != null)
                {
                    waiterUI.IsReady = waiter.IsReady;
                }
            }
        }

        private void CreateNewWaiters(IEnumerable<LobbyWaiterSendData> toCreate, VisualElement waiterList)
        {
            foreach (LobbyWaiterSendData waiter in toCreate)
            {
                CreateWaiter(waiter, waiterList);
            }
        }

        private void CreateWaiter(LobbyWaiterSendData waiter, VisualElement waiterList)
        {
            LobbyWaiter lobbyWaiter = new LobbyWaiter();
            lobbyWaiter.IsReady = waiter.IsReady;
            lobbyWaiter.PlayerName = $"Player: {waiter.WaiterID.ToString()}";
            lobbyWaiter.PlayerColor = waiter.WaiterColor;
            lobbyWaiter.name = waiter.WaiterID.ToString();
            
            waiterList.Add(lobbyWaiter);
        }

        private void CleanUp()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            VisualElement waiterList = root.Q<VisualElement>(_waiterListName);
            
            RemoveMissings(_waitersCache, waiterList);
            _waitersCache = null;
        }
        
    }
}

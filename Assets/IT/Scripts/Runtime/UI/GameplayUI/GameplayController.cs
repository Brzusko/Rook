using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IT.ScriptableObjects;
using IT.ScriptableObjects.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace IT.UI
{
    public class GameplayController : Controller
    {
        private static readonly string _playerIndicatorsID = "PlayerIndicators";
        private static readonly string _gameplayLabelID = "GameplayLabel";
        
        [SerializeField] private GameplayBinding _gameplayBinding;
        [SerializeField] private GameSettings _gameSettings;
        
        private HashSet<int> _ids = new();

        public override ControllerIDs ControllerID => ControllerIDs.GAMEPLAY;

        protected override void Awake()
        {
            base.Awake();

            VisualElement root = _uiDocument.rootVisualElement;
            Label gameplayLabel = root.Q<Label>(_gameplayLabelID);
            gameplayLabel.text = $"Points to win: {_gameSettings.PointsToWin.ToString()}";
            
            _gameplayBinding.ClearUI += OnClearUI;
            _gameplayBinding.CreatePlayerIndicator += OnCreatePlayerIndicator;
            _gameplayBinding.RemovePlayerIndicator += OnRemovePlayerIndicator;
            _gameplayBinding.UpdatePoints += OnUpdatePoints;
            _gameplayBinding.SetMain += OnSetMain;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _gameplayBinding.ClearUI -= OnClearUI;
            _gameplayBinding.CreatePlayerIndicator -= OnCreatePlayerIndicator;
            _gameplayBinding.RemovePlayerIndicator -= OnRemovePlayerIndicator;
            _gameplayBinding.UpdatePoints -= OnUpdatePoints;
            _gameplayBinding.SetMain -= OnSetMain;
        }

        private void OnClearUI()
        {
            if(_ids.Count == 0)
                return;
            
            _ids.ToList().ForEach(OnRemovePlayerIndicator);
            _ids.Clear();
        }
        
        private void OnCreatePlayerIndicator(int id, Color color)
        {
            if(_ids.Contains(id))
                return;
            
            VisualElement root = _uiDocument.rootVisualElement;
            VisualElement playerIndicators = root.Q<VisualElement>(_playerIndicatorsID);

            PlayerIndicator playerIndicator = new PlayerIndicator();

            playerIndicator.name = id.ToString();
            playerIndicator.PlayerColor = color;
            playerIndicator.IsMain = false;
            playerIndicator.FillPercent = 0f;
            playerIndicator.PlayerPoints = 0;
            
            playerIndicators.Add(playerIndicator);
            _ids.Add(id);
        }
        
        private void OnRemovePlayerIndicator(int id)
        {
            if(!_ids.Contains(id))
                return;
            
            VisualElement root = _uiDocument.rootVisualElement;
            VisualElement playerIndicators = root.Q<VisualElement>(_playerIndicatorsID);

            PlayerIndicator playerIndicator = playerIndicators.Q<PlayerIndicator>(id.ToString());
            
            playerIndicators.Remove(playerIndicator);

            _ids.Remove(id);
        }
        
        private void OnUpdatePoints(int id, int points)
        {
            if(!_ids.Contains(id))
                return;

            PlayerIndicator playerIndicator = GetPlayerIndicatorAt(id);
            playerIndicator.PlayerPoints = points;
        }
        
        private void OnSetMain(int id)
        {
            if(!_ids.Contains(id))
                return;
            
            PlayerIndicator playerIndicator = GetPlayerIndicatorAt(id);
            playerIndicator.IsMain = true;
        }

        private PlayerIndicator GetPlayerIndicatorAt(int id)
        {
            if (!_ids.Contains(id))
                return null;
            
            VisualElement root = _uiDocument.rootVisualElement;
            VisualElement playerIndicators = root.Q<VisualElement>(_playerIndicatorsID);

            return playerIndicators.Q<PlayerIndicator>(id.ToString());
        }

    }
}

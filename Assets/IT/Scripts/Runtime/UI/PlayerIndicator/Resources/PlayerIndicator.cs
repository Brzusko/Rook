using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace IT.UI
{
    public class PlayerIndicator : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PlayerIndicator, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlColorAttributeDescription playerColorAttr = new UxmlColorAttributeDescription
            {
                name = "Player Color"
            };

            private UxmlIntAttributeDescription pointsAmountAttr = new UxmlIntAttributeDescription
            {
                name = "Points Amount"
            };

            private UxmlFloatAttributeDescription fillPercentAttr = new UxmlFloatAttributeDescription
            {
                name = "Fill %"
            };

            private UxmlBoolAttributeDescription isMainAttr = new UxmlBoolAttributeDescription()
            {
                name = "Is Main"
            };
            
            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext context)
            {
                base.Init(visualElement, bag, context);

                PlayerIndicator playerIndicator = visualElement as PlayerIndicator;
                playerIndicator.PlayerColor = playerColorAttr.GetValueFromBag(bag, context);
                playerIndicator.PlayerPoints = pointsAmountAttr.GetValueFromBag(bag, context);
                playerIndicator.FillPercent = fillPercentAttr.GetValueFromBag(bag, context);
                playerIndicator.IsMain = isMainAttr.GetValueFromBag(bag, context);
            }
        }

        private static readonly string _styles = "PlayerIndicatorStyles";
        
        private static readonly string _mainContainerClass = "player-container";
        private static readonly string _playerColorClass = "player-color";
        private static readonly string _playerPointsContainerClass = "player-points-container";
        private static readonly string _playerPointsLabelClass = "player-points-label";
        private static readonly string _playerProgressbarContainerClass = "player-progressbar-container";
        private static readonly string _playerProgressbarFillClass = "player-progressbar-fill";

        private Color _color;
        private int _points;
        private float _fill;
        private bool _isMain;

        private VisualElement _mainContainer;
        private readonly VisualElement _playerColor;
        private readonly Label _playerPoints;
        private readonly VisualElement _progressbarFill;
        
        public Color PlayerColor
        {
            get => _color;
            set
            {
                _color = value;
                if (_playerColor != null)
                    _playerColor.style.backgroundColor = _color;
            }
        }

        public int PlayerPoints
        {
            get => _points;
            set
            {
                _points = value;
                if (_playerPoints != null)
                    _playerPoints.text = $"{_points.ToString()}";
            }
        }

        public float FillPercent
        {
            get => _fill;
            set
            {
                _fill = Mathf.Clamp(value, 0, 100);
                if (_progressbarFill != null)
                    _progressbarFill.style.width = new Length(_fill, LengthUnit.Percent);
            }
        }

        public bool IsMain
        {
            get => _isMain;
            set
            {
                _isMain = value;
                if (_mainContainer != null)
                {
                    StyleColor styleColor = _mainContainer.style.backgroundColor;
                    Color bgColor = styleColor.value;
                    bgColor.a = _isMain ? 0.2f : 0f;
                    styleColor.value = bgColor;
                    _mainContainer.style.backgroundColor = bgColor;
                }
            }
        }

        public PlayerIndicator()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(_styles));
            _mainContainer = new VisualElement();
            _mainContainer.AddToClassList(_mainContainerClass);
            hierarchy.Add(_mainContainer);

            _playerColor = new VisualElement();
            _playerColor.AddToClassList(_playerColorClass);

            VisualElement playerPointsContainer = new VisualElement();
            playerPointsContainer.AddToClassList(_playerPointsContainerClass);
            
            _mainContainer.Add(_playerColor);
            _mainContainer.Add(playerPointsContainer);

            _playerPoints = new Label("3000");
            _playerPoints.AddToClassList(_playerPointsLabelClass);
            
            playerPointsContainer.Add(_playerPoints);

            VisualElement progressbarContainer = new VisualElement();
            progressbarContainer.AddToClassList(_playerProgressbarContainerClass);
            
            playerPointsContainer.Add(progressbarContainer);

            _progressbarFill = new VisualElement();
            _progressbarFill.AddToClassList(_playerProgressbarFillClass);
            progressbarContainer.Add(_progressbarFill);
        }
    }
}

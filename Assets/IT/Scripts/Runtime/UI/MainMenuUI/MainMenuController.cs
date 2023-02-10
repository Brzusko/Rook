using System;
using System.Collections;
using System.Collections.Generic;
using IT.ScriptableObjects.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace IT.UI
{
    public class MainMenuController : Controller
    {
        [SerializeField] private MainMenuBinding _mainMenuBinding;
        public override ControllerIDs ControllerID => ControllerIDs.MAIN_MENU;

        protected override void BindUI()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            Button playButton = root.Q<Button>("Play");
            Button exitButton = root.Q<Button>("Exit");

            playButton.clicked += _mainMenuBinding.InvokePlayButton;
            exitButton.clicked += _mainMenuBinding.InvokeExitButton;
        }

        protected override void UnbindUI()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            Button playButton = root.Q<Button>("Play");
            Button exitButton = root.Q<Button>("Exit");
            
            playButton.clicked -= _mainMenuBinding.InvokePlayButton;
            exitButton.clicked -= _mainMenuBinding.InvokeExitButton;
        }
    }
}

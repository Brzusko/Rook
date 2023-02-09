using System;
using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using IT.ScriptableObjects.UI;
using IT.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Gameplay
{
    public class MainMenuHandler : MonoBehaviour
    {
        [SerializeField] private MainMenuBinding _mainMenuBinding;

        private void Awake()
        {
            ServiceContainer.Get<IUI>().ShowUI(ControllerIDs.MAIN_MENU, true);
        }

        private void OnEnable()
        {
            _mainMenuBinding.PlayButtonClicked += OnPlayButton;
            _mainMenuBinding.ExitButtonClicked += OnExitButton;
        }

        private void OnDisable()
        {
            _mainMenuBinding.PlayButtonClicked -= OnPlayButton;
            _mainMenuBinding.ExitButtonClicked -= OnExitButton;
        }

        private void OnDestroy()
        {
            ServiceContainer.Get<IUI>().HideUI(ControllerIDs.MAIN_MENU);
        }

        private void Update()
        {
            if (Keyboard.current.shiftKey.isPressed && Keyboard.current.enterKey.isPressed)
            {
                ServiceContainer.Get<INetworkBridge>().StartServer();
                ServiceContainer.Get<INetworkBridge>().StartClient();
            }
        }

        private void OnPlayButton()
        {
            ServiceContainer.Get<INetworkBridge>().StartClient();
        }

        private void OnExitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

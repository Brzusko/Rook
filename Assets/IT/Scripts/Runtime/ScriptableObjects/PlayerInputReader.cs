using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "IT/Input/Input Reader")]
    public class PlayerInputReader : ScriptableObject, MainInput.IGameplayActions, MainInput.ICameraActions
    {
        public event Action<Vector2> CameraRotation;
        public event Action FreeLookRequest;

        private MainInput _mainInput;
        private Vector2 _movementInput = Vector2.zero;

        public NetworkedInput NetworkedInput => new NetworkedInput { MovementInput = _movementInput };

        private void OnEnable()
        {
            ConstructInput();
        }

        private void OnDisable()
        {
            DisableGameplayInput();
            DisableCameraInput();
        }

        private void ConstructInput()
        {
            if(_mainInput != null)
                return;

            _mainInput = new MainInput();
            _mainInput.Gameplay.SetCallbacks(this);
            _mainInput.Camera.SetCallbacks(this);
            EnableCameraInput();
            EnableGameplayInput();
        }

        public void EnableGameplayInput()
        {
            _mainInput.Gameplay.Enable();
        }

        public void DisableGameplayInput()
        {
            _mainInput.Gameplay.Disable();
        }

        public void EnableCameraInput()
        {
            _mainInput.Camera.Enable();
        }

        public void DisableCameraInput()
        {
            _mainInput.Camera.Disable();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            _movementInput = context.ReadValue<Vector2>();
        }

        public void OnCameraMovement(InputAction.CallbackContext context)
        {
            if(context.phase != InputActionPhase.Performed)
                return;
            
            CameraRotation?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnRequestFreeLook(InputAction.CallbackContext context)
        {
            if(context.phase != InputActionPhase.Performed)
                return;
            
            FreeLookRequest?.Invoke();
        }
    }

    public struct NetworkedInput
    {
        public Vector2 MovementInput;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Data.Networking;
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
        private Vector2 _pointerPositionInput = Vector2.zero;
        private bool _isWalkingPressed;

        public Vector2 MovementInput => _movementInput;
        public Vector2 PointerPosition => _pointerPositionInput;
        public bool IsWalkingPressed => _isWalkingPressed;

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

        public void OnPointerMovement(InputAction.CallbackContext context)
        {
            _pointerPositionInput = context.ReadValue<Vector2>();
        }

        public void OnWalking(InputAction.CallbackContext context)
        {
            _isWalkingPressed = context.phase == InputActionPhase.Performed;
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
}

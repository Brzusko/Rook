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

        public Vector2 MovementInput { get; private set; } = Vector2.zero;

        public Vector2 PointerPosition { get; private set; } = Vector2.zero;

        public bool IsWalkingPressed { get; private set; }

        public bool IsJumpPressed { get; private set; }
        public bool IsMainActionPressed { get; private set; }
        public bool IsSecondaryActionPressed { get; private set; }

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
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnPointerMovement(InputAction.CallbackContext context)
        {
            PointerPosition = context.ReadValue<Vector2>();
        }

        public void OnWalking(InputAction.CallbackContext context)
        {
            IsWalkingPressed = context.phase == InputActionPhase.Performed;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            IsJumpPressed = context.phase == InputActionPhase.Performed;
        }

        public void OnMainAction(InputAction.CallbackContext context)
        {
            IsMainActionPressed = context.phase == InputActionPhase.Performed;
        }

        public void OnSecondaryAction(InputAction.CallbackContext context)
        {
            IsSecondaryActionPressed = context.phase == InputActionPhase.Performed;
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

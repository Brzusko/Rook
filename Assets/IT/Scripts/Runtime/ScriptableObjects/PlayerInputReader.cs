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
    public class PlayerInputReader : ScriptableObject, MainInput.IGameplayActions
    {
        private MainInput _mainInput;
        private bool _isForwardPressed;
        private bool _isBackwardPressed;
        private bool _isLeftPressed;
        private bool _isRightPressed;

        public NetworkedInput NetworkedInput => new NetworkedInput
        (
            _isForwardPressed, 
            _isBackwardPressed, 
            _isLeftPressed,
            _isRightPressed
        );
        
        private void OnEnable()
        {
            ConstructInput();
        }

        private void OnDisable()
        {
            DisableGameplayInput();
        }

        private void ConstructInput()
        {
            if(_mainInput != null)
                return;

            _mainInput = new MainInput();
            _mainInput.Gameplay.SetCallbacks(this);
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

        public void OnMoveForward(InputAction.CallbackContext context)
        {
            _isForwardPressed = context.phase == InputActionPhase.Performed;
        }

        public void OnMoveBackward(InputAction.CallbackContext context)
        {
            _isBackwardPressed = context.phase == InputActionPhase.Performed;
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            _isLeftPressed = context.phase == InputActionPhase.Performed;
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            _isRightPressed = context.phase == InputActionPhase.Performed;
        }
    }

    public readonly struct NetworkedInput
    {
        private readonly byte _input;
        public NetworkedInput(bool forwardState, bool backwardState, bool leftState, bool rightState)
        {
            _input = 0;
            _input |= forwardState ? Constants.FORWARD_FLAG : Constants.NULL;
            _input |= backwardState ? Constants.BACKWARD_FLAG : Constants.NULL;
            _input |= leftState ? Constants.LEFT_FLAG : Constants.NULL;
            _input |= rightState ? Constants.RIGHT_FLAG : Constants.NULL;
        }

        public Vector2 DecodeMovementInput()
        {
            int forward = (_input & Constants.FORWARD_FLAG) > Constants.NULL ? 1 : 0;
            int backward = (_input & Constants.BACKWARD_FLAG) > Constants.NULL ? -1 : 0;
            int left = (_input & Constants.LEFT_FLAG) > Constants.NULL ? -1 : 0;
            int right = (_input & Constants.RIGHT_FLAG) > Constants.NULL ? 1 : 0;
            return new Vector2((left + right), (forward + backward)).normalized;
        }
    }
}

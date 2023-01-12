using System;
using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using IT.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Controllers
{
    public class FreeLookController : MonoBehaviour, IFreeLookController
    {
        [SerializeField] private CharacterMovement _movement;
        [SerializeField] private float _maxSpeed = 10f;
        [SerializeField] private float _acceleration = 30f;

        private Vector3 _desiredVelocity;

        public Transform CameraFollowTarget => transform;
        public Transform CameraLookAtTarget => transform;
        
        private void Update()
        {
            Movement();
        }
    
        private Input BuildInput()
        {
            Vector2 movementVector = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            
            if(keyboard.wKey.isPressed)
                movementVector += Vector2.up;
            
            if(keyboard.sKey.isPressed)
                movementVector += Vector2.down;
            
            if(keyboard.aKey.isPressed)
                movementVector += Vector2.left;

            if (keyboard.dKey.isPressed)
                movementVector += Vector2.right;

            movementVector = movementVector.normalized;

            return new Input
            {
                MovementVector = movementVector,
                IsControlPressed = keyboard.ctrlKey.isPressed,
                IsShiftPressed = keyboard.shiftKey.isPressed,
                IsSpacePressed = keyboard.spaceKey.isPressed
            };
        }

        private void Movement()
        {
            Input input = BuildInput();
            
            if(_movement.isConstrainedToGround)
                _movement.PauseGroundConstraint();

            float acceleration = _acceleration * Time.deltaTime;
            
            float downForce = input.IsControlPressed ? -_maxSpeed : 0;
            float upForce = input.IsSpacePressed ? _maxSpeed : 0;
            float flyingForce = downForce + upForce;

            float xComponent = Mathf.MoveTowards(_desiredVelocity.x, input.MovementVector.x * _maxSpeed, acceleration);
            float yComponent = Mathf.MoveTowards(_desiredVelocity.y, flyingForce, acceleration);
            float zComponent = Mathf.MoveTowards(_desiredVelocity.z, input.MovementVector.y * _maxSpeed, acceleration);
            
            _desiredVelocity.x = xComponent;
            _desiredVelocity.y = yComponent;
            _desiredVelocity.z = zComponent;

            _movement.Move(_desiredVelocity, Time.deltaTime);
        }
        
        public void ChangeState(bool state)
        {
            enabled = state;
        }
        
        private struct Input
        {
            public Vector2 MovementVector;
            public bool IsControlPressed;
            public bool IsSpacePressed;
            public bool IsShiftPressed;
        }
        
    }
}

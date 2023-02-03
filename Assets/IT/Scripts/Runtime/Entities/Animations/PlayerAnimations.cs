using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Animancer;
using EasyCharacterMovement;
using FishNet.Managing.Timing;
using IT.Collections;
using IT.Data.Networking;
using UnityEditor.Animations;

namespace IT
{
    public class PlayerAnimations : NetworkBehaviour
    {
        [SerializeField] private Transform _mainSpaceTransform;
        [SerializeField] private Animator _animator;
        private Dictionary<PlayerMovementAnimID, int> _animationMovementHashes;
        private Dictionary<PlayerCombatAnimID, int> _animationCombatHashes;

        private PlayerMovementAnimID _currentMovementAnimationID;
        private PlayerCombatAnimID _currentCombatAnimationID;

        private void Awake()
        {
            InitializeOnce();
        }

        private void InitializeOnce()
        {
            _animationMovementHashes = new Dictionary<PlayerMovementAnimID, int>
            {
                {
                    PlayerMovementAnimID.JUMPING, Animator.StringToHash("Jump Start")
                },
                {
                    PlayerMovementAnimID.FALLING, Animator.StringToHash("Jump Mid")
                },
                {
                    PlayerMovementAnimID.LANDING, Animator.StringToHash("Jump Land")
                },
                {
                    PlayerMovementAnimID.GROUNDED, Animator.StringToHash("Grounded Movement")
                }
            };

            _animationCombatHashes = new Dictionary<PlayerCombatAnimID, int>
            {
                {
                    PlayerCombatAnimID.NONE, Animator.StringToHash("Combat Idle")
                },
                {
                    PlayerCombatAnimID.PREPARE_SWING, Animator.StringToHash("Prepare Swing")
                },
                {
                    PlayerCombatAnimID.SWING, Animator.StringToHash("Swing")
                },
                {
                    PlayerCombatAnimID.BLOCK, Animator.StringToHash("Block")
                }
            };
        }

        public void PlayAnimation(PlayerMovementAnimID movementAnimID, float transitionTime = 0f)
        {
            if(!_animationMovementHashes.ContainsKey(movementAnimID) || _currentMovementAnimationID == movementAnimID)
                return;

            _currentMovementAnimationID = movementAnimID;
            _animator.CrossFade(_animationMovementHashes[movementAnimID], transitionTime, 0);
        }

        public void PlayAnimation(PlayerCombatAnimID combatAnimID, float transitionTime = 0f)
        {
            if(!_animationCombatHashes.ContainsKey(combatAnimID) || _currentCombatAnimationID == combatAnimID)
                return;

            _currentCombatAnimationID = combatAnimID;
            _animator.CrossFade(_animationCombatHashes[combatAnimID], transitionTime);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "IT/Input/Input Reader")]
    public class PlayerInputReader : ScriptableObject
    {
        private MainInput _mainInput;

        private void OnEnable()
        {
            if (_mainInput == null)
            {
                
            }
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
        }

        public void EnableGameplayInput()
        {
            
        }

        public void DisableGameplayInput()
        {
            
        }
    }
}

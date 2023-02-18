using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Utils
{
    [Serializable]
    public class SimpleTimer
    {
        public event Action Complete;
        
        [SerializeField]
        private float _currentTime;
        private float _timeToComplete;
        public bool IsComplete => _currentTime >= _timeToComplete;

        public void Update(float deltaTime)
        {
            if(IsComplete)
                return;

            _currentTime += deltaTime;
            
            if(IsComplete)
                Complete?.Invoke();
        }

        public void Start(float timeToComplete)
        {
            _currentTime = 0f;
            _timeToComplete = timeToComplete;
        }
    }
}

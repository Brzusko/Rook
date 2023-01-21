using System;
using FishNet.Example.Prediction.Rigidbodies;

using System.Collections;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

namespace IT
{
    public class RigidbodyLaunchpad : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if(InstanceFinder.PredictionManager.IsReplaying()) return;
            if (other.TryGetComponent(out RigidbodyPrediction rigidbodyPrediction))
            {
                rigidbodyPrediction.TriggerEnter = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(InstanceFinder.PredictionManager.IsReplaying()) return;
            if (other.TryGetComponent(out RigidbodyPrediction rigidbodyPrediction))
            {
                rigidbodyPrediction.TriggerEnter = false;
            }
        }
    }
}

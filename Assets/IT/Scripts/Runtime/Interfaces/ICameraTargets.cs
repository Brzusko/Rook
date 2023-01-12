using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface ICameraTargets
    {
        public Transform CameraFollowTarget { get; }
        public Transform CameraLookAtTarget { get; }
    }
}

using System.Collections;
using System.Collections.Generic;
using FishNet.Object.Prediction;
using UnityEngine;

namespace IT.Data.Networking
{
    public struct NetworkInput : IReplicateData
    {
        public Vector2 MovementInput;
        public float YRotation;

        #region Default

        private uint _tick;
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public void Dispose()
        {
        }

        #endregion
    }
}

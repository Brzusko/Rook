using System.Collections;
using System.Collections.Generic;
using FishNet.Object.Prediction;
using IT.FSM;
using UnityEngine;

namespace IT.Data.Networking
{
    public struct NetworkInput : IReplicateData
    {
        public Vector2 MovementInput;
        public float YRotation;
        public bool IsWalkingPressed;
        public bool IsJumpPressed;
        public bool IsMainActionPressed;
        public bool IsSecondaryActionPressed;
        public uint SimulationTick;
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

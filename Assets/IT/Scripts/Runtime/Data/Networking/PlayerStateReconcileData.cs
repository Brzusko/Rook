using FishNet.Object.Prediction;
using IT.FSM;
using UnityEngine;

namespace IT.Data.Networking
{
    public struct PlayerStateReconcileData : IReconcileData
    {
        
        public Vector3 Velocity;
        public Vector3 Position;
        public Vector3 GroundNormal;
        public Quaternion Rotation;
        public bool IsConstrainedToGround;
        public float UnconstrainedTimer;
        public bool HitGround;
        public bool IsWalkable;
        public PlayerStateID StateID;
        public float AdditionalMovementMultiplier;
        
        #region Default
        private uint _tick;
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
        public void Dispose() {}
        #endregion

    }
}

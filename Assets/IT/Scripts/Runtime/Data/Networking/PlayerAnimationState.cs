using System.Collections;
using System.Collections.Generic;
using FishNet.Serializing;
using UnityEngine;

namespace IT.Data.Networking
{

    public enum PlayerAnimationStateID : byte
    {
        NONE,
        GROUNDED,
        FALLING,
        LANDING,
    }
    
    public struct PlayerAnimationState
    {
        public Vector2 AnimationVector;
        public uint Tick;
        public float Speed;
        public float Duration;
        public PlayerAnimationStateID StateID;
    }

    public static class PlayerAnimationStateSerializer
    {
        public static void WritePlayerAnimationState(this Writer writer, PlayerAnimationState value)
        {
            short xAnimComponent = (short)(value.AnimationVector.x * 100f);
            short yAnimComponent = (short)(value.AnimationVector.y * 100f);
            
            writer.WriteInt16(xAnimComponent);
            writer.WriteInt16(yAnimComponent);
            writer.WriteUInt32(value.Tick, AutoPackType.Unpacked);
            writer.WriteByte((byte)value.StateID);
            
            if(value.StateID != PlayerAnimationStateID.GROUNDED)
                return;

            byte speed = (byte)(value.Speed * 100f);
            byte duration = (byte)(value.Duration * 10f);
            
            writer.WriteByte(speed);
            writer.WriteByte(duration);
        }

        public static PlayerAnimationState ReadPlayerAnimationState(this Reader reader)
        {
            float xAnimComponent = (reader.ReadInt16() / 100f);
            float yAnimComponent = (reader.ReadInt16() / 100f);
            Vector2 animVector = new Vector2(xAnimComponent, yAnimComponent);
            uint tick = reader.ReadUInt32(AutoPackType.Unpacked);
            PlayerAnimationStateID stateID = (PlayerAnimationStateID)reader.ReadByte();
            PlayerAnimationState playerAnimationState = new PlayerAnimationState { AnimationVector = animVector, Tick = tick, StateID = stateID};

            if (stateID == PlayerAnimationStateID.GROUNDED)
            {
                float speed = reader.ReadByte() * 10f;
                float duration = reader.ReadByte() * 10f;

                playerAnimationState.Speed = speed;
                playerAnimationState.Duration = duration;
            }
            
            return playerAnimationState;
        }
    }

    public static class PlayerAnimationStateIDUtils
    {
        public static bool ShouldBeProcessed(this PlayerAnimationStateID stateID)
        {
            return stateID == PlayerAnimationStateID.GROUNDED;
        }
    }
}

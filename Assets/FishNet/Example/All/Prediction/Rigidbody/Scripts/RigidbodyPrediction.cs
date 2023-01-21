using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

/*
* 
* See TransformPrediction.cs for more detailed notes.
* 
*/

namespace FishNet.Example.Prediction.Rigidbodies
{

    public class RigidbodyPrediction : NetworkBehaviour
    {
        #region Types.
        public struct MoveData : IReplicateData
        {
            public bool Jump;
            public float Horizontal;
            public float Vertical;
            public bool TriggerEnter;
            public MoveData(bool jump, float horizontal, float vertical, bool triggerEnter)
            {
                Jump = jump;
                Horizontal = horizontal;
                Vertical = vertical;
                _tick = 0;
                TriggerEnter = triggerEnter;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
        public struct ReconcileData : IReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;
            public float AdditionalForce;
            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, float additionalForce)
            {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                AngularVelocity = angularVelocity;
                _tick = 0;
                AdditionalForce = additionalForce;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
        #endregion

        #region Serialized.
        [SerializeField]
        private float _jumpForce = 15f;
        [SerializeField]
        private float _moveRate = 15f;
        #endregion

        #region Private.
        /// <summary>
        /// Rigidbody on this object.
        /// </summary>
        private Rigidbody _rigidbody;
        /// <summary>
        /// Next time a jump is allowed.
        /// </summary>
        private float _nextJumpTime;
        /// <summary>
        /// True to jump next frame.
        /// </summary>
        private bool _jump;
        #endregion

        public bool LaunchUP;
        public bool TriggerEnter;
        public float AdditionalUPForce;

        private void Awake()
        {

            _rigidbody = GetComponent<Rigidbody>();
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
            }
        }

        private void Update()
        {
            if (base.IsOwner)
            {
                if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextJumpTime)
                {
                    _nextJumpTime = Time.time + 1f;
                    _jump = true;
                }
            }
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconciliation(default, false);
                CheckInput(out MoveData md);
                Move(md, false);
            }
            if (base.IsServer)
            {
                Move(default, true);
            }
        }


        private void TimeManager_OnPostTick()
        {
            if (base.IsServer)
            {
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity, AdditionalUPForce);
                Reconciliation(rd, true);
            }

            LaunchUP = false;
        }

        private void CheckInput(out MoveData md)
        {
            md = default;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (horizontal == 0f && vertical == 0f && !_jump && !TriggerEnter)
                return;

            md = new MoveData(_jump, horizontal, vertical, TriggerEnter);
            _jump = false;
        }

        [Replicate]
        private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {
            //Add extra gravity for faster falls.
            Vector3 forces = new Vector3(md.Horizontal, Physics.gravity.y, md.Vertical) * _moveRate;
            _rigidbody.AddForce(forces);

            float deltaTime = (float)base.TimeManager.TickDelta;

            if (asServer)
            {
                AdditionalUPForce = Mathf.MoveTowards(AdditionalUPForce, TriggerEnter ? 160f : 0f, 100f * deltaTime);
            }
            else
            {
                AdditionalUPForce = Mathf.MoveTowards(AdditionalUPForce, md.TriggerEnter ? 160f : 0f, 100f * deltaTime);
            }

            if (AdditionalUPForce > 0)
            {
                _rigidbody.AddForce(Vector3.up * AdditionalUPForce);
            }
            
            if (md.Jump)
                _rigidbody.AddForce(new Vector3(0f, _jumpForce, 0f), ForceMode.Impulse);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _rigidbody.velocity = rd.Velocity;
            _rigidbody.angularVelocity = rd.AngularVelocity;
            AdditionalUPForce = rd.AdditionalForce;
        }


    }


}
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

namespace EasyCharacterMovement.CharacterMovementExamples.NetworkingExamples.FishNetExamples
{
    /// <summary>
    /// This example show how move a networked character with full client side prediction and reconciliation.
    /// This follows the same approach as the FishNet Transform / CharacterController prediction examples.
    /// </summary>

    public class PlayerController : NetworkBehaviour
    {
        // #region STRUCTS
        //
        // public struct InputData
        // {
        //     public const uint BUTTON_USE = 1 << 0;
        //     public const uint BUTTON_FIRE = 1 << 1;
        //     public const uint BUTTON_FIRE_ALT = 1 << 2;
        //
        //     public const uint BUTTON_FORWARD = 1 << 3;
        //     public const uint BUTTON_BACKWARD = 1 << 4;
        //     public const uint BUTTON_LEFT = 1 << 5;
        //     public const uint BUTTON_RIGHT = 1 << 6;
        //
        //     public const uint BUTTON_JUMP = 1 << 7;
        //     public const uint BUTTON_CROUCH = 1 << 8;
        //     public const uint BUTTON_WALK = 1 << 9;
        //
        //     public const uint BUTTON_ACTION1 = 1 << 10;
        //     public const uint BUTTON_ACTION2 = 1 << 11;
        //     public const uint BUTTON_ACTION3 = 1 << 12;
        //     public const uint BUTTON_ACTION4 = 1 << 14;
        //
        //     public const uint BUTTON_RELOAD = 1 << 15;
        //
        //     public uint buttons;
        //
        //     public bool IsPressed(uint button) => (buttons & button) == button;
        //     public bool IsReleased(uint button) => IsPressed(button) == false;
        // }
        //
        // public struct CharacterMovementReconcileData
        // {
        //     public Vector3 position;
        //     public Quaternion rotation;
        //
        //     public Vector3 velocity;
        //
        //     public bool isConstrainedToGround;
        //     public float unconstrainedTimer;
        //
        //     public bool hitGround;
        //     public bool isWalkable;
        //
        //     public Vector3 groundNormal;
        // }
        //
        // #endregion
        //
        // #region EDITOR EXPOSED FIELDS
        //
        // public float rotationRate = 540.0f;
        //
        // public float maxSpeed = 5;
        //
        // public float acceleration = 20.0f;
        // public float deceleration = 20.0f;
        //
        // public float groundFriction = 8.0f;
        // public float airFriction = 0.5f;
        //
        // public float jumpImpulse = 6.5f;
        //
        // [Range(0.0f, 1.0f)]
        // public float airControl = 0.3f;
        //
        // public Vector3 gravity = Vector3.down * 9.81f;
        //
        // #endregion
        //
        // #region PROPERTIES
        //
        // public CharacterMovement characterMovement { get; private set; }
        //
        // public Vector3 moveDirection { get; set; }
        //
        // public bool jump { get; set; }
        //
        // #endregion
        //
        // #region METHODS
        //
        // /// <summary>
        // /// Cache components.
        // /// </summary>
        //
        // private void CacheComponents()
        // {
        //     characterMovement = GetComponent<CharacterMovement>();
        // }
        //
        // /// <summary>
        // /// Read player inputs.
        // /// </summary>
        //
        // private void ReadInput(out InputData inputData)
        // {
        //     inputData = default;
        //
        //     if (Input.GetKey(KeyCode.W))
        //     {
        //         inputData.buttons |= InputData.BUTTON_FORWARD;
        //     }
        //
        //     if (Input.GetKey(KeyCode.S))
        //     {
        //         inputData.buttons |= InputData.BUTTON_BACKWARD;
        //     }
        //
        //     if (Input.GetKey(KeyCode.A))
        //     {
        //         inputData.buttons |= InputData.BUTTON_LEFT;
        //     }
        //
        //     if (Input.GetKey(KeyCode.D))
        //     {
        //         inputData.buttons |= InputData.BUTTON_RIGHT;
        //     }
        //
        //     if (Input.GetKey(KeyCode.Space))
        //     {
        //         inputData.buttons |= InputData.BUTTON_JUMP;
        //     }
        //
        //     if (Input.GetKey(KeyCode.C))
        //     {
        //         inputData.buttons |= InputData.BUTTON_CROUCH;
        //     }
        //
        //     if (Input.GetKey(KeyCode.E))
        //     {
        //         inputData.buttons |= InputData.BUTTON_ACTION1;
        //     }
        //
        //     if (Input.GetKey(KeyCode.Q))
        //     {
        //         inputData.buttons |= InputData.BUTTON_ACTION2;
        //     }
        //
        //     if (Input.GetKey(KeyCode.F))
        //     {
        //         inputData.buttons |= InputData.BUTTON_ACTION3;
        //     }
        //
        //     if (Input.GetKey(KeyCode.G))
        //     {
        //         inputData.buttons |= InputData.BUTTON_ACTION4;
        //     }
        //
        //     if (Input.GetKey(KeyCode.R))
        //     {
        //         inputData.buttons |= InputData.BUTTON_RELOAD;
        //     }
        //
        //     if (Input.GetMouseButton(0))
        //     {
        //         inputData.buttons |= InputData.BUTTON_FIRE;
        //     }
        // }
        //
        // /// <summary>
        // /// Handle player input.
        // /// </summary>
        //
        // private void HandleInput(InputData inputData)
        // {
        //     // Movement input
        //
        //     Vector3 inputMoveDirection = Vector3.zero;
        //     if (inputData.IsPressed(InputData.BUTTON_FORWARD))
        //         inputMoveDirection += Vector3.forward;
        //
        //     if (inputData.IsPressed(InputData.BUTTON_BACKWARD))
        //         inputMoveDirection += Vector3.back;
        //
        //     if (inputData.IsPressed(InputData.BUTTON_LEFT))
        //         inputMoveDirection += Vector3.left;
        //
        //     if (inputData.IsPressed(InputData.BUTTON_RIGHT))
        //         inputMoveDirection += Vector3.right;
        //
        //     moveDirection = Vector3.ClampMagnitude(inputMoveDirection, 1.0f);
        //
        //     // Jump input
        //
        //     jump = inputData.IsPressed(InputData.BUTTON_JUMP);
        // }
        //
        // /// <summary>
        // /// Update character's rotation.
        // /// </summary>
        //
        // private void UpdateRotation()
        // {
        //     // Rotate towards movement direction
        //
        //     float deltaTime = (float)TimeManager.TickDelta;
        //     characterMovement.RotateTowards(moveDirection, rotationRate * deltaTime);
        // }
        //
        // /// <summary>
        // /// Update character's movement.
        // /// </summary>
        //
        // private void UpdateMovement()
        // {
        //     // Jumping
        //
        //     if (jump && characterMovement.isGrounded)
        //     {
        //         characterMovement.PauseGroundConstraint();
        //         characterMovement.LaunchCharacter(Vector3.up * jumpImpulse, true);
        //     }
        //
        //     // Do move
        //
        //     Vector3 desiredVelocity = moveDirection * maxSpeed;
        //
        //     float actualAcceleration = characterMovement.isGrounded ? acceleration : acceleration * airControl;
        //     float actualDeceleration = characterMovement.isGrounded ? deceleration : 0.0f;
        //
        //     float actualFriction = characterMovement.isGrounded ? groundFriction : airFriction;
        //
        //     float deltaTime = (float)TimeManager.TickDelta;
        //
        //     characterMovement.SimpleMove(desiredVelocity, maxSpeed, actualAcceleration, actualDeceleration,
        //         actualFriction, actualFriction, gravity, true, deltaTime);
        // }
        //
        // /// <summary>
        // /// Simulate character using given input data.
        // /// </summary>
        //
        // [Replicate]
        // private void Simulate(InputData inputData, bool asServer, bool replying = false)
        // {
        //     HandleInput(inputData);
        //     UpdateRotation();
        //     UpdateMovement();
        // }
        //
        // [Reconcile]
        // private void Reconciliation(CharacterMovementReconcileData reconcileData, bool asServer)
        // {
        //     CharacterMovement.State characterState = new CharacterMovement.State(
        //         reconcileData.position,
        //         reconcileData.rotation,
        //         reconcileData.velocity,
        //         reconcileData.isConstrainedToGround,
        //         reconcileData.unconstrainedTimer,
        //         reconcileData.hitGround,
        //         reconcileData.isWalkable,
        //         reconcileData.groundNormal);
        //
        //     characterMovement.SetState(in characterState);
        // }
        //
        // #endregion
        //
        // #region NETWORKBEHAVIOUR
        //
        // public override void OnStartClient()
        // {
        //     base.OnStartClient();
        //
        //     // Subscribe to TimeManager events
        //
        //     TimeManager.OnTick += OnTick;
        //
        //     // Take control of player camera
        //
        //     if (!IsOwner)
        //         return;
        //
        //     GameObject camera = GameObject.Find("Player Camera");
        //     if (camera != null)
        //     {
        //         if (camera.TryGetComponent<CharacterMovementDemo.SimpleCameraController>(out var cameraController))
        //         {
        //             cameraController.target = transform.Find("Root/Camera Follow");
        //             cameraController.enabled = cameraController.target != null;
        //         }
        //     }
        // }
        //
        // public override void OnStopClient()
        // {
        //     base.OnStopClient();
        //
        //     // Unsubscribe from TimeManager events
        //
        //     TimeManager.OnTick -= OnTick;
        // }
        //
        // private void OnTick()
        // {
        //     if (IsOwner)
        //     {
        //         Reconciliation(default, false);
        //         ReadInput(out InputData inputData);
        //         Simulate(inputData, false);
        //     }
        //
        //     if (IsServer)
        //     {
        //         Simulate(default, true);
        //
        //         CharacterMovement.State characterState = characterMovement.GetState();
        //         CharacterMovementReconcileData reconcileData = new CharacterMovementReconcileData
        //         {
        //             position = characterState.position,
        //             rotation = characterState.rotation,
        //
        //             velocity = characterState.velocity,
        //
        //             isConstrainedToGround = characterState.isConstrainedToGround,
        //             unconstrainedTimer = characterState.unconstrainedTimer,
        //
        //             isWalkable = characterState.isWalkable,
        //             hitGround = characterState.hitGround,
        //
        //             groundNormal = characterState.groundNormal
        //         };
        //
        //         Reconciliation(reconcileData, true);
        //     }
        // }
        //
        // #endregion
        //
        // #region MONOBEHAVIOUR
        //
        // private void Awake()
        // {
        //     CacheComponents();
        // }
        //
        // #endregion
    }
}

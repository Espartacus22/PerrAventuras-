using Fusion;
using UnityEngine;
using Networking;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetPlayerCamera))]
public class NetPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.75f;

    [SerializeField] private float rotationSpeed = 12f;

    private CharacterController controller;
    private NetPlayerCamera netPlayerCamera;
    private float verticalVelocity;

    [Networked] private TickTimer DashTimer { get; set; }
    [Networked] private TickTimer DashCooldownTimer { get; set; }
    [Networked] private Vector3 DashDirection { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        netPlayerCamera = GetComponent<NetPlayerCamera>();
    }

    public override void Spawned()
    {
        Debug.Log($"Net player spawned | InputAuth: {HasInputAuthority} | StateAuth: {HasStateAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetworkInputPlayer>(out var inputData))
            return;

        // Base de cámara para movimiento:
        // - Local: usa la cámara real del jugador
        // - Remoto/servidor: usa la dirección enviada en el input
        Vector3 camForward;

        if (HasInputAuthority && netPlayerCamera != null)
        {
            camForward = netPlayerCamera.GetCameraPlanarForward();
        }
        else
        {
            camForward = inputData.lookDirection - transform.position;
            camForward.y = 0f;

            if (camForward.sqrMagnitude < 0.001f)
                camForward = transform.forward;

            camForward.Normalize();
        }

        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        Vector3 move = camForward * inputData.moveInput.y + camRight * inputData.moveInput.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (inputData.buttons.IsSet(NetworkInputPlayer.JUMP))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity += gravity * Runner.DeltaTime;
        }

        bool canStartDash = !DashTimer.IsRunning && !DashCooldownTimer.IsRunning;
        bool hasMoveInput = move.sqrMagnitude > 0.001f;

        if (inputData.buttons.IsSet(NetworkInputPlayer.DASH) && canStartDash && hasMoveInput)
        {
            DashDirection = move.normalized;
            DashTimer = TickTimer.CreateFromSeconds(Runner, dashDuration);
            DashCooldownTimer = TickTimer.CreateFromSeconds(Runner, dashCooldown);
        }

        float currentSpeed = moveSpeed;

        if (inputData.buttons.IsSet(NetworkInputPlayer.RUN))
            currentSpeed *= runMultiplier;

        Vector3 horizontalVelocity;

        if (DashTimer.IsRunning)
        {
            horizontalVelocity = DashDirection * dashSpeed;

            if (DashTimer.Expired(Runner))
                DashTimer = TickTimer.None;
        }
        else
        {
            horizontalVelocity = move * currentSpeed;
        }

        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Runner.DeltaTime);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Runner.DeltaTime
            );
        }

        if (DashCooldownTimer.IsRunning && DashCooldownTimer.Expired(Runner))
        {
            DashCooldownTimer = TickTimer.None;
        }
    }
}

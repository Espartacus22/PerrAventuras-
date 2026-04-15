using Fusion;
using UnityEngine;
using Networking;

[RequireComponent(typeof(CharacterController))]
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

    private CharacterController controller;
    private float verticalVelocity;

    [Networked] private TickTimer DashTimer { get; set; }
    [Networked] private TickTimer DashCooldownTimer { get; set; }
    [Networked] private Vector3 DashDirection { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void Spawned()
    {
        Debug.Log($"Net player spawned | InputAuth: {HasInputAuthority} | StateAuth: {HasStateAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetInputData>(out var inputData))
            return;

        Vector3 move = new Vector3(inputData.move.x, 0f, inputData.move.y);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -0.01f;

            if (inputData.buttons.IsSet(NetInputData.JUMP))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity += gravity * Runner.DeltaTime;
        }

        // Iniciar dash
        bool canStartDash = !DashTimer.IsRunning && !DashCooldownTimer.IsRunning;
        bool hasMoveInput = move.sqrMagnitude > 0.001f;

        if (inputData.buttons.IsSet(NetInputData.DASH) && canStartDash && hasMoveInput)
        {
            DashDirection = move;
            DashTimer = TickTimer.CreateFromSeconds(Runner, dashDuration);
            DashCooldownTimer = TickTimer.CreateFromSeconds(Runner, dashCooldown);
        }

        float currentSpeed = moveSpeed;

        if (inputData.buttons.IsSet(NetInputData.RUN))
            currentSpeed *= runMultiplier;

        Vector3 horizontalVelocity;

        if (DashTimer.IsRunning)
        {
            horizontalVelocity = DashDirection * dashSpeed;

            if (DashTimer.Expired(Runner))
            {
                DashTimer = TickTimer.None;
            }
        }
        else
        {
            horizontalVelocity = move * currentSpeed;
        }

        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Runner.DeltaTime);

        if (DashCooldownTimer.IsRunning && DashCooldownTimer.Expired(Runner))
        {
            DashCooldownTimer = TickTimer.None;
        }
    }
}

using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using Networking;

[RequireComponent(typeof(NetworkRigidbody3D))] // CRÍTICO para networking
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Datos del personaje")]
    public CharacterType characterData;

    [Header("Ground")]
    public LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 1.2f;
    [SerializeField] private float groundRayOffset = 0.1f;

    [Header("Saltos y Gravedad")]
    [SerializeField] private bool hasDoubleJump = false;
    [SerializeField] private float extraFallGravity = 2.5f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [SerializeField] private Transform gameplayCamera;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler inputHandler;

    public bool isGrounded;
    private bool isCrouching;
    private bool isDashing;
    private int jumpCount;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 dashDirection;

    public PStateMachine StateMachine { get; private set; }
    public PIdleState IdleState { get; private set; }
    public PMoveState MoveState { get; private set; }
    public PRunState RunState { get; private set; }
    public PJumpState JumpState { get; private set; }
    public PDashState DashState { get; private set; }
    public PCrouchState CrouchState { get; private set; }

    public PlayerInputHandler InputHandler => inputHandler;
    public CharacterType CharacterData => characterData;
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => rb.linearVelocity.y;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        inputHandler = GetComponent<PlayerInputHandler>();

        originalHeight = capsule.height;
        originalCenter = capsule.center;
        rb.freezeRotation = true;

        StateMachine = new PStateMachine();
        IdleState = new PIdleState(this, StateMachine);
        MoveState = new PMoveState(this, StateMachine);
        RunState = new PRunState(this, StateMachine);
        JumpState = new PJumpState(this, StateMachine);
        DashState = new PDashState(this, StateMachine);
        CrouchState = new PCrouchState(this, StateMachine);
    }

    // --- AQUÍ ESTÁ LA MAGIA FUSIONADA ---
    public override void Spawned()
    {
        // 1. Inicializamos tu máquina de estados
        StateMachine.Initialize(IdleState);

        // 2. Conectamos la cámara
        if (HasInputAuthority)
        {
            // Forzamos "UnityEngine.Object" para evitar confusiones, 
            // y usamos el nuevo nombre de Unity 6: "CinemachineCamera"
            var vcam = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();

            if (vcam != null)
            {
                vcam.Follow = this.transform;
                vcam.LookAt = this.transform;
                Debug.Log("[CAMARA] ¡Conectada al jugador local en red!");
            }

            if (Camera.main != null)
            {
                gameplayCamera = Camera.main.transform;
                Debug.Log("[CAMARA] GameplayCamera asignada al player local.");
            }
        }
    }
   

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (characterData == null) return;

        // Leemos el input de la red (lo envía el NetworkController)
        if (GetInput(out NetworkInputPlayer input))
        {
            // Alimentamos al títere (InputHandler) para que la máquina de estados funcione intacta
            inputHandler.MoveInput = input.moveInput;
            inputHandler.JumpPressed = input.buttons.IsSet(NetworkInputPlayer.JUMP);
            inputHandler.RunHeld = input.buttons.IsSet(NetworkInputPlayer.RUN);
            inputHandler.DashPressed = input.buttons.IsSet(NetworkInputPlayer.DASH);
            inputHandler.CrouchPressed = input.buttons.IsSet(NetworkInputPlayer.CROUCH);
            inputHandler.MeleePressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0);
            inputHandler.RangedPressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1);

            UpdateGroundCheck();
            RotateTowardsNetwork(input.lookDirection);

            // La máquina de estados original hace su magia sin saber que está en red
            StateMachine.CurrentState.LogicUpdate();
            StateMachine.CurrentState.PhysicsUpdate();

            HandleBetterGravity();
        }
    }

    private void UpdateGroundCheck()
    {
        bool wasGrounded = isGrounded;
        Vector3 origin = transform.position + Vector3.up * groundRayOffset;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    public bool HasMovementInput() => inputHandler.MoveInput.sqrMagnitude > 0.01f;

    public void Move(bool running)
    {
        if (isDashing) return;

        Vector3 input = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        if (input.sqrMagnitude <= 0.01f)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 moveDir = GetCameraRelativeDirection(input);
        float speed = characterData.walkSpeed;

        if (isCrouching) speed *= characterData.crouchMultiplier;
        else if (running) speed *= characterData.runMultiplier;

        rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed);
    }

    public void MoveInAir()
    {
        Vector3 input = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        if (input.sqrMagnitude <= 0.01f) return;

        Vector3 moveDir = GetCameraRelativeDirection(input);
        float speed = characterData.walkSpeed * 0.8f;
        rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed);
    }

    public void MoveCrouched() => Move(false);

    public void StopHorizontalMovement()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    public void Jump()
    {
        int maxJumps = hasDoubleJump ? 2 : 1;
        if (jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, characterData.jumpForce, rb.linearVelocity.z);
            jumpCount++;
        }
    }

    public void BeginDash()
    {
        isDashing = true;
        Vector3 input = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        dashDirection = GetCameraRelativeDirection(input);
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
    }

    public void DashMove()
    {
        rb.linearVelocity = new Vector3(dashDirection.x * characterData.dashSpeed, rb.linearVelocity.y, dashDirection.z * characterData.dashSpeed);
    }

    public void EndDash() => isDashing = false;

    public void StartCrouch()
    {
        if (isCrouching) return;
        isCrouching = true;
        float newHeight = originalHeight * characterData.crouchHeight;
        capsule.height = newHeight;
        float heightDelta = (originalHeight - newHeight) * 0.5f;
        capsule.center = originalCenter - new Vector3(0f, heightDelta, 0f);
    }

    public void StopCrouch()
    {
        if (!isCrouching) return;
        isCrouching = false;
        capsule.height = originalHeight;
        capsule.center = originalCenter;
    }

    private Vector3 GetCameraRelativeDirection(Vector3 input)
    {
        Transform cam = gameplayCamera;

        if (cam == null)
        {
            return input.normalized;
        }

        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized;
        return moveDir;
    }

    private void RotateTowardsNetwork(Vector3 lookPosition)
    {
        Vector3 lookDir = lookPosition - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, characterData.rotationSpeed * Runner.DeltaTime);
        }
    }

    private void HandleBetterGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (extraFallGravity - 1f) * Runner.DeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !inputHandler.JumpPressed)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpGravityMultiplier - 1f) * Runner.DeltaTime;
        }
    }

    public void UnlockDoubleJump()
    {
        hasDoubleJump = true;
        Debug.Log($"{name}: Doble salto desbloqueado.");
    }

    public void LockDoubleJump()
    {
        hasDoubleJump = false;
        Debug.Log($"{name}: Doble salto bloqueado.");
    }

    public bool HasDoubleJump()
    {
        return hasDoubleJump;
    }
}
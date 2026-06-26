using Fusion;
using Networking;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Datos del personaje")]
    public CharacterType characterData;
    private NetworkCharacterController ncc;
    private CharacterController unityCC;
    private PlayerInputHandler inputHandler;
    private bool isCrouching;
    private bool isDashing;
    private int jumpCount;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 dashDirection;

    private NetworkButtons _prevButtons;

    public int JumpCount => jumpCount;
    private bool _unlockedDoubleJump = false;

    // --- MÁQUINA DE ESTADOS ---
    public PStateMachine StateMachine { get; private set; }
    public PIdleState IdleState { get; private set; }
    public PMoveState MoveState { get; private set; }
    public PRunState RunState { get; private set; }
    public PJumpState JumpState { get; private set; }
    public PDashState DashState { get; private set; }
    public PCrouchState CrouchState { get; private set; }

    public PlayerInputHandler InputHandler => inputHandler;
    public CharacterType CharacterData => characterData;
    public bool IsGrounded => ncc.Grounded;
    public float VerticalVelocity => ncc.Velocity.y;
    public Vector3 CurrentVelocity => ncc.Velocity;

    private void Awake()
    {
        ncc = GetComponent<NetworkCharacterController>();
        unityCC = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        originalHeight = unityCC.height;
        originalCenter = unityCC.center;

        StateMachine = new PStateMachine();
        IdleState = new PIdleState(this, StateMachine);
        MoveState = new PMoveState(this, StateMachine);
        RunState = new PRunState(this, StateMachine);
        JumpState = new PJumpState(this, StateMachine);
        DashState = new PDashState(this, StateMachine);
        CrouchState = new PCrouchState(this, StateMachine);
    }

    public override void Spawned()
    {
        StateMachine.Initialize(IdleState);
        if (characterData != null)
        {
            ncc.maxSpeed = characterData.walkSpeed;
            ncc.jumpImpulse = characterData.jumpForce;
            ncc.rotationSpeed = characterData.rotationSpeed;
        }

        if (HasInputAuthority)
        {
            var vcam = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
            if (vcam != null)
            {
                vcam.Follow = this.transform;
                vcam.LookAt = this.transform;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (characterData == null) return;

        if (GetInput(out NetworkInputPlayer input))
        {
            inputHandler.MoveInput = input.moveInput;
            inputHandler.RunHeld = input.buttons.IsSet(NetworkInputPlayer.RUN);
            inputHandler.CrouchPressed = input.buttons.IsSet(NetworkInputPlayer.CROUCH);
            inputHandler.RangedPressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1);
            inputHandler.MeleePressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0);

            NetworkButtons pressedButtons = input.buttons.GetPressed(_prevButtons);
            inputHandler.JumpPressed = pressedButtons.IsSet(NetworkInputPlayer.JUMP);
            inputHandler.DashPressed = pressedButtons.IsSet(NetworkInputPlayer.DASH);
            _prevButtons = input.buttons;

            if (ncc.Grounded && ncc.Velocity.y <= 0.01f) jumpCount = 0;

            Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
            bool isAiming = inputHandler.RangedPressed || inputHandler.MeleePressed;

            if (isAiming)
            {
                Vector3 forwardDir = moveDir.sqrMagnitude > 0.01f ? moveDir : transform.forward;
                Vector3 mouseDir = input.lookDirection - transform.position;
                mouseDir.y = 0;
                float angle = Vector3.Angle(forwardDir, mouseDir);

                if (angle < 60f && mouseDir.sqrMagnitude > 0.1f) RotateTowardsNetwork(transform.position + mouseDir);
                else if (moveDir.sqrMagnitude > 0.01f) RotateTowardsNetwork(transform.position + moveDir);
            }
            else if (moveDir.sqrMagnitude > 0.01f) RotateTowardsNetwork(transform.position + moveDir);
        }

        StateMachine.CurrentState.LogicUpdate();
        StateMachine.CurrentState.PhysicsUpdate();

        // --- COMUNICACIÓN ANIMATOR OPTIMIZADA PARA FUSION ---
        // Obtenemos el componente de red que maneja la sincronización
        var netMecanim = GetComponent<NetworkMecanimAnimator>();

        // Si existe y tiene un Animator asignado, actualizamos el parámetro a través de la red
        if (netMecanim != null && netMecanim.Animator != null)
        {
            float speed = ncc.Velocity.magnitude / ncc.maxSpeed;
            // Al llamar al Animator del componente de red, Fusion sincroniza el cambio
            netMecanim.Animator.SetFloat("Speed", speed);
        }
    }

    public bool HasMovementInput() => inputHandler.MoveInput.sqrMagnitude > 0.01f;

    public void Move(bool running)
    {
        if (isDashing) return;
        Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        float currentSpeed = characterData.walkSpeed;
        if (isCrouching) currentSpeed *= characterData.crouchMultiplier;
        else if (running) currentSpeed *= characterData.runMultiplier;
        ncc.maxSpeed = currentSpeed;
        if (moveDir.sqrMagnitude <= 0.01f) { ncc.Move(Vector3.zero); return; }
        ncc.Move(moveDir);
    }

    public void MoveInAir()
    {
        if (isDashing) return;
        Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        ncc.maxSpeed = characterData.walkSpeed * 0.8f;
        ncc.Move(moveDir.sqrMagnitude > 0.01f ? moveDir : Vector3.zero);
    }

    public void MoveCrouched() => Move(false);
    public void StopHorizontalMovement() => ncc.Move(Vector3.zero);

    public void Jump()
    {
        int maxJumps = HasDoubleJump() ? 2 : 1;
        if (jumpCount < maxJumps)
        {
            Vector3 vel = ncc.Velocity;
            vel.y = 0;
            ncc.Velocity = vel;
            ncc.Jump(true, characterData.jumpForce);
            jumpCount++;
        }
    }

    public void BeginDash()
    {
        isDashing = true;
        dashDirection = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
    }

    public void DashMove()
    {
        float velocidadNormal = ncc.maxSpeed;
        ncc.maxSpeed = 1000f;
        Vector3 dashVel = dashDirection.normalized * characterData.dashSpeed;
        if (!ncc.Grounded) dashVel.y = -ncc.Velocity.y;
        ncc.Move(dashVel);
        ncc.maxSpeed = velocidadNormal;
    }

    public void EndDash()
    {
        isDashing = false;
        ncc.Move(Vector3.zero);
    }

    public void StartCrouch()
    {
        if (isCrouching) return;
        isCrouching = true;
        float newHeight = originalHeight * characterData.crouchHeight;
        unityCC.height = newHeight;
        float heightDelta = (originalHeight - newHeight) * 0.5f;
        unityCC.center = originalCenter - new Vector3(0f, heightDelta, 0f);
    }

    public void StopCrouch()
    {
        if (!isCrouching) return;
        isCrouching = false;
        unityCC.height = originalHeight;
        unityCC.center = originalCenter;
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

    public void UnlockDoubleJump() => _unlockedDoubleJump = true;
    public void LockDoubleJump() => _unlockedDoubleJump = false;
    public bool HasDoubleJump() => (characterData != null && characterData.dobleSalto) || _unlockedDoubleJump;
}


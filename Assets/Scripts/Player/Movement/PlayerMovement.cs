using Fusion;
using Networking;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Datos del personaje")]
    public CharacterType characterData; // El ScriptableObject

    private NetworkCharacterController ncc;
    private CharacterController unityCC;
    private PlayerInputHandler inputHandler;

    private bool isCrouching;
    private bool isDashing;
    private int jumpCount;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 dashDirection;

    // Variable para recordar si un NPC nos regaló el salto en esta partida
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

        // Volcamos los valores iniciales del Scriptable Object al CC de red
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
            inputHandler.JumpPressed = input.buttons.IsSet(NetworkInputPlayer.JUMP);
            inputHandler.RunHeld = input.buttons.IsSet(NetworkInputPlayer.RUN);
            inputHandler.DashPressed = input.buttons.IsSet(NetworkInputPlayer.DASH);
            inputHandler.CrouchPressed = input.buttons.IsSet(NetworkInputPlayer.CROUCH);
            inputHandler.MeleePressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0);
            inputHandler.RangedPressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1);

            if (ncc.Grounded)
            {
                jumpCount = 0;
            }

            // --- LÓGICA DE ROTACIÓN INTELIGENTE ---
            bool isAiming = inputHandler.RangedPressed || inputHandler.MeleePressed;

            if (isAiming)
            {
                // Si ataca, mira al mouse (strafing)
                RotateTowardsNetwork(input.lookDirection);
            }
            else
            {
                // Si no ataca, mira hacia donde camina
                Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    RotateTowardsNetwork(transform.position + moveDir);
                }
            }

            StateMachine.CurrentState.LogicUpdate();
            StateMachine.CurrentState.PhysicsUpdate();
        }
    }

    public bool HasMovementInput() => inputHandler.MoveInput.sqrMagnitude > 0.01f;

    public void Move(bool running)
    {
        if (isDashing) return;

        // El vector ya viene mundializado desde el NetworkController
        Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);

        float currentSpeed = characterData.walkSpeed;
        if (isCrouching) currentSpeed *= characterData.crouchMultiplier;
        else if (running) currentSpeed *= characterData.runMultiplier;

        ncc.maxSpeed = currentSpeed;

        if (moveDir.sqrMagnitude <= 0.01f)
        {
            ncc.Move(Vector3.zero);
            return;
        }

        ncc.Move(moveDir);
    }

    public void MoveInAir()
    {
        if (isDashing) return;

        Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        ncc.maxSpeed = characterData.walkSpeed * 0.8f; // Penalización ligera en el aire

        if (moveDir.sqrMagnitude > 0.01f)
        {
            ncc.Move(moveDir);
        }
        else
        {
            ncc.Move(Vector3.zero);
        }
    }

    public void MoveCrouched() => Move(false);

    public void StopHorizontalMovement()
    {
        ncc.Move(Vector3.zero);
    }

    public void Jump()
    {
        int maxJumps = HasDoubleJump() ? 2 : 1;

        if (jumpCount < maxJumps)
        {
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
        Vector3 dashVel = dashDirection * characterData.dashSpeed;
        ncc.Velocity = new Vector3(dashVel.x, 0f, dashVel.z);
        unityCC.Move(ncc.Velocity * Runner.DeltaTime);
    }

    public void EndDash() => isDashing = false;

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

    // --- MÉTODOS DE DESBLOQUEO DE SALTO (Para los NPCs) ---

    public void UnlockDoubleJump()
    {
        _unlockedDoubleJump = true;
        Debug.Log($"{name}: Doble salto desbloqueado por NPC/Evento.");
    }

    public void LockDoubleJump()
    {
        _unlockedDoubleJump = false;
        Debug.Log($"{name}: Doble salto bloqueado.");
    }

    public bool HasDoubleJump()
    {
        // Verifica si lo tiene de base en el SO, o si un NPC se lo dio
        return (characterData != null && characterData.dobleSalto) || _unlockedDoubleJump;
    }
}
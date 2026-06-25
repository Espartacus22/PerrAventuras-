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

    // --- VARIABLE DE RED CONTROL DE IMPULSOS ---
    private NetworkButtons _prevButtons; // Guarda los botones del tick anterior para detectar el click inicial

    public int JumpCount
    {
        get { return jumpCount; }
    }

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

    // Propiedad pública para que tus scripts de disparo puedan leer la velocidad real actual
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
            // 1. Sincronización de inputs continuos (Mantener apretado WASD, Correr, Agacharse)
            inputHandler.MoveInput = input.moveInput;
            inputHandler.RunHeld = input.buttons.IsSet(NetworkInputPlayer.RUN);
            inputHandler.CrouchPressed = input.buttons.IsSet(NetworkInputPlayer.CROUCH);
            inputHandler.RangedPressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1);
            inputHandler.MeleePressed = input.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0);

            // --- EL TRUCO DE RED (Detección de Taps únicos) ---
            // Calculamos qué botones se ACABAN DE PULSAR en este frame de red comparado con el anterior
            NetworkButtons pressedButtons = input.buttons.GetPressed(_prevButtons);

            // Asignamos los inputs de acción usando únicamente el pulso (GetPressed)
            // Esto evita loops infinitos si el jugador deja el dedo apoyado en el botón
            inputHandler.JumpPressed = pressedButtons.IsSet(NetworkInputPlayer.JUMP);
            inputHandler.DashPressed = pressedButtons.IsSet(NetworkInputPlayer.DASH);

            // Guardamos el estado actual para la comparación del próximo tick de red
            _prevButtons = input.buttons;

            // Protección de reseteo del salto en el suelo de forma nativa
            if (ncc.Grounded && ncc.Velocity.y <= 0.01f)
            {
                jumpCount = 0;
            }

            // --- 2. LÓGICA DE ROTACIÓN CON CAMPO DE VISIÓN (FOV) ---
            Vector3 moveDir = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
            bool isAiming = inputHandler.RangedPressed || inputHandler.MeleePressed;

            if (isAiming)
            {
                Vector3 forwardDir = moveDir.sqrMagnitude > 0.01f ? moveDir : transform.forward;
                Vector3 mouseDir = input.lookDirection - transform.position;
                mouseDir.y = 0;

                float angle = Vector3.Angle(forwardDir, mouseDir);

                if (angle < 60f && mouseDir.sqrMagnitude > 0.1f)
                {
                    RotateTowardsNetwork(transform.position + mouseDir);
                }
                else if (moveDir.sqrMagnitude > 0.01f)
                {
                    RotateTowardsNetwork(transform.position + moveDir);
                }
            }
            else if (moveDir.sqrMagnitude > 0.01f)
            {
                RotateTowardsNetwork(transform.position + moveDir);
            }
        }

        // --- 3. PROCESAMIENTO DE ESTADOS Y FÍSICA ---
        StateMachine.CurrentState.LogicUpdate();
        StateMachine.CurrentState.PhysicsUpdate();
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
        ncc.maxSpeed = characterData.walkSpeed * 0.8f;

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
            // Si es el segundo salto legítimo (jumpCount == 1),
            // aplicamos un contra-impulso vertical para limpiar la caída y reiniciar la altura limpia.
            if (jumpCount > 0)
            {
                ncc.Move(new Vector3(0f, characterData.jumpForce - ncc.Velocity.y, 0f));
            }
            else
            {
                ncc.Jump(true, characterData.jumpForce);
            }

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
        // 1. Guardamos la velocidad máxima original para restaurarla después
        float velocidadNormal = ncc.maxSpeed;

        // 2. Desbloqueamos el techo de velocidad: 
        // Le damos un valor altísimo para que no recorte nuestro Dash de 500
        ncc.maxSpeed = 1000f;

        // 3. Calculamos la velocidad deseada
        Vector3 dashVel = dashDirection.normalized * characterData.dashSpeed;

        // 4. Compensamos gravedad si estamos en el aire
        if (!ncc.Grounded)
        {
            dashVel.y = -ncc.Velocity.y;
        }

        // 5. Aplicamos el movimiento
        ncc.Move(dashVel);

        // 6. IMPORTANTE: Restauramos el techo original para no romper el movimiento normal
        ncc.maxSpeed = velocidadNormal;
    }

    public void EndDash()
    {
        isDashing = false;
        // Cortamos el impulso del dash al instante al salir del estado
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
        return (characterData != null && characterData.dobleSalto) || _unlockedDoubleJump;
    }
}
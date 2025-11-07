using UnityEngine;


/// <summary>
/// PlayerLocal: control offline del jugador (movimiento, salto, dash, agacharse, correr, ataques).
/// Requiere: CharacterController, PlayerHealthLocal, PlayerInputHandler y un CharacterType ScriptableObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerHealthLocal))]
public class PlayerLocal : MonoBehaviour
{
    [Header("Referencias")]
    public CharacterType characterData;
    public PlayerInputHandler input;
    public ProjectileLocal projectile;
    public CharacterController controller;

    [Header("Movimiento")]
    public float moveSpeed = 10f;
    public float runMultiplier = 1.5f;
    public float crouchSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -9.8f;
    public float dashSpeed = 25f;
    public float dashDuration = 0.5f;

    [Header("Ground Detection")]
    public float groundDistance = 1.1f;
    public LayerMask groundMask;
    public bool isGrounded;

    [Header("Habilidades Desbloqueadas")]
    public bool dashUnlocked = true;
    public bool doubleJumpUnlocked = false;
    public bool blockUnlocked = false;

    [Header("Special Movement")]
    public bool canLevitate = false; // Solo el personaje 1 por el momento

    [HideInInspector] public int jumpCount = 0;

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public bool hasDoubleJumped;
    [HideInInspector] public float verticalVelocity = 0f;
    public float JumpForce = 5f;

    public StateMachine StateMachine { get; private set; }

    private Vector3 velocity; // acumulador vertical (y)
    private Vector2 lastMoveInput;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        projectile = GetComponent<ProjectileLocal>();
        controller = GetComponent<CharacterController>();
        StateMachine = new StateMachine();
        currentSpeed = moveSpeed;
    }

    private void Start()
    {
        StateMachine.ChangeState(new PlayerMoveState(this));
    }

    private void Update()
    {
        CheckGround();
        ApplyGravity();
        StateMachine.Update();
    }

    // -------- MOVE --------
    public void Move(Vector2 inputMove)
    {
        lastMoveInput = inputMove;

        // Dirección calculada de forma segura (usa input helper si existe)
        Vector3 moveDir = GetMoveDirection(inputMove);
        // normalizar sólo si hay input para evitar borrar vertical
        if (moveDir.sqrMagnitude > 1e-4f) moveDir.Normalize();

        // Aplicar movimiento horizontal + vertical en un solo controller.Move
        Vector3 motion = moveDir * currentSpeed;
        controller.Move(motion * Time.deltaTime);
    }

    private Vector3 GetMoveDirection(Vector2 inputMove)
    {
        if (input != null)
        {
            
            try
            {
                return input.GetMoveDirectionRelativeToCamera(inputMove);
            }
            catch (System.Exception)
            {
                // fallback abajo
            }
        }

        // fallback: calcula respecto a la cámara principal
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam == null)
        {
            return new Vector3(inputMove.x, 0f, inputMove.y);
        }

        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cam.right; right.y = 0f; right.Normalize();
        return right * inputMove.x + forward * inputMove.y;
    }

    public Vector3 GetDirectionFromMovement(Vector2 moveInput)
    {
        // Si existe input handler con helper, usalo
        if (input != null)
        {
            try
            {
                return input.GetMoveDirectionRelativeToCamera(moveInput).normalized;
            }
            catch (System.Exception)
            {
                // fallback abajo
            }
        }

        // fallback: usar camera main
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam == null)
            return new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cam.right; right.y = 0f; right.Normalize();
        Vector3 dir = right * moveInput.x + forward * moveInput.y;
        if (dir.sqrMagnitude < 1e-6f) dir = transform.forward;
        dir.y = 0f;
        return dir.normalized;
    }

    // -------- JUMP --------
    public void Jump()
    {
        // Si estamos en el suelo, iniciamos el primer salto
        if (isGrounded)
        {
            jumpCount = 1;
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasDoubleJumped = false;
            Debug.Log($"{gameObject.name} Salto inicial (jumpCount={jumpCount})");
        }
        // Si no estamos en suelo, solo permitimos doble salto si está desbloqueado y aún no lo usó
        else if (doubleJumpUnlocked && jumpCount < 2)
        {
            jumpCount = 2;
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasDoubleJumped = true;
            Debug.Log($"{gameObject.name} Doble salto (jumpCount={jumpCount})");
        }
    }

    // -------- Crouch --------
    public void Crouch(bool isCrouching)
    {
        float targetHeight = isCrouching ? 1.2f : 2f;
        float smooth = 10f;

        float oldHeight = controller.height;
        Vector3 oldCenter = controller.center;

        // Punto inferior del collider (el pie) — calculado desde oldCenter/oldHeight
        float bottomY = oldCenter.y - (oldHeight / 2f);

        // Suavizar cambio de altura
        float newHeight = Mathf.Lerp(oldHeight, targetHeight, Time.deltaTime * smooth);
        controller.height = newHeight;

        // Recalcular y reasignar center para mantener la base fija (evita hundimiento)
        Vector3 newCenter = oldCenter;
        newCenter.y = bottomY + newHeight / 2f;
        controller.center = newCenter;

        currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
    }

    // -------- RUN --------
    public void Run(bool isRunning)
    {
        currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;
    }

    // -------- GRAVITY --------
    public void ApplyGravity()
    {
        if (isGrounded)
        {
            // si está en suelo, fijamos una pequeña velocidad hacia abajo para mantener contacto
            if (velocity.y < 0f)
                velocity.y = -5f;

            // reset del contador de saltos al tocar suelo
            jumpCount = 0;
            hasDoubleJumped = false;
        }
        else
        {
            // aplicar gravedad
            velocity.y += gravity * Time.deltaTime;

            // si NO puede levitar, forzamos una caída más realista con tope
            if (!canLevitate && velocity.y < -30f)
                velocity.y = -30f;
        }

        // Aplicar vertical únicamente si no hay movimiento horizontal ya aplicado en Move()
        // Si Move(...) fue llamado este frame, ésta moverá solo vertical adicional.
        controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
    }

    // -------- CHECK GROUND --------
    public void CheckGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, groundDistance + 0.1f, groundMask))
        {
            isGrounded = true;
            jumpCount = 0;
        }
        else
        {
            isGrounded = false;
        }
    }

    // -------- SHOOT --------
    public void Shoot()
    {
        if (projectile != null)
            projectile.Shoot();
    }

    // -------- Ability (PlayerLevel) --------
    public void EnableDash(bool value) => dashUnlocked = value;
    public void EnableDoubleJump(bool value) => doubleJumpUnlocked = value;
    public void EnableBlock(bool value) => blockUnlocked = value;

}
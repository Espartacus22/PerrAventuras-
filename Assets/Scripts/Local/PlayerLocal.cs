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

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public bool hasDoubleJumped;

    public StateMachine StateMachine { get; private set; }

    private Vector3 velocity;

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
        StateMachine.Update();
        ApplyGravity();
    }

    // -------- MOVIMIENTO GENERAL --------
    public void Move(Vector2 inputMove)
    {
        if (input == null) return;

        // dirección según cámara
        Vector3 moveDir = input.GetMoveDirectionRelativeToCamera(inputMove);
        moveDir.Normalize();

        controller.Move(moveDir * currentSpeed * Time.deltaTime);
    }

    // -------- SALTO --------
    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasDoubleJumped = false;
            Debug.Log($"{gameObject.name} Salto inicial");
        }
        else if (doubleJumpUnlocked && !hasDoubleJumped)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasDoubleJumped = true;
            Debug.Log($"{gameObject.name} Doble salto");
        }
    }

    // -------- AGACHARSE --------
    public void Crouch(bool isCrouching)
    {
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;

            // Ajusta altura sin hundir al personaje
            float targetHeight = 1.2f;
            float currentHeight = controller.height;
            controller.height = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * 10f);

            // Ajusta el centro sin empujar hacia abajo
            controller.center = new Vector3(0, controller.height / 2f, 0);
        }
        else
        {
            currentSpeed = moveSpeed;
            float targetHeight = 2f;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);
            controller.center = new Vector3(0, controller.height / 2f, 0);
        }
    }

    // -------- CORRER --------
    public void Run(bool isRunning)
    {
        currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;
    }

    // -------- GRAVEDAD --------
    public void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -4f; // más firme al suelo
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // -------- DETECCIÓN DE SUELO --------
    public void CheckGround()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundDistance, groundMask);
        isGrounded = grounded;
        Debug.DrawRay(transform.position, Vector3.down * groundDistance, grounded ? Color.green : Color.red);

        // Reiniciar doble salto solo si está tocando suelo
        if (isGrounded)
        {
            hasDoubleJumped = false;
            velocity.y = -2f;
        }
        else
        {
            // Aplica gravedad si no hay suelo
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    // -------- DISPARO --------
    public void Shoot()
    {
        if (projectile != null)
            projectile.Shoot();
    }

    // -------- HABILIDADES (PlayerLevel) --------
    public void EnableDash(bool value) => dashUnlocked = value;
    public void EnableDoubleJump(bool value) => doubleJumpUnlocked = value;
    public void EnableBlock(bool value) => blockUnlocked = value;
}
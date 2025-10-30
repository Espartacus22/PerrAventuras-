using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerLocal: control offline del jugador (movimiento, salto, dash, agacharse, correr, ataques).
/// Requiere: CharacterController, PlayerHealthLocal, PlayerInputHandler y un CharacterType ScriptableObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerHealthLocal))]
public class PlayerLocal : MonoBehaviour
{
    [Header("References")]
    public CharacterType characterData;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public PlayerInputHandler input;
    [HideInInspector] public ProjectileLocal projectile;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 9f;
    public float gravity = -20f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Ground Raycast")]
    public float groundDistance = 1.0f;
    public LayerMask groundMask;
    [HideInInspector] public bool isGrounded;

    // state
    public StateMachine StateMachine { get; private set; }

    // internal
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public int jumpCount = 0;
    public int maxJumps = 2;

    [Header("Habilidades Desbloqueadas (runtime)")]
    public bool dashUnlocked = false;
    public bool doubleJumpUnlocked = false;
    public bool blockUnlocked = false;

    void Start()
    {
        // inicializa los estados
        StateMachine.Initialize(new PlayerMoveState(this));
    }

    /// <summary>Habilita o deshabilita dash desde PlayerLevel</summary>
    public void EnableDash(bool value)
    {
        dashUnlocked = value;
        Debug.Log($"EnableDash: {value}");
    }

    /// <summary>Habilita o deshabilita doble salto desde PlayerLevel</summary>
    public void EnableDoubleJump(bool value)
    {
        doubleJumpUnlocked = value;
        Debug.Log($"EnableDoubleJump: {value}");
    }

    /// <summary>Habilita o deshabilita bloqueo desde PlayerLevel</summary>
    public void EnableBlock(bool value)
    {
        blockUnlocked = value;
        Debug.Log($"EnableBlock: {value}");
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        projectile = GetComponent<ProjectileLocal>();

        StateMachine = new StateMachine();
    }

    

    void Update()
    {
        CheckGround();
        StateMachine.Update();
    }

    public void Move(Vector2 moveInput)
    {
        Vector3 moveDir = input != null ? input.GetMoveDirectionRelativeToCamera(moveInput)
                                        : new Vector3(moveInput.x, 0f, moveInput.y);

        controller.Move(moveDir * moveSpeed * Time.deltaTime);
    }

    public void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        jumpCount++;
    }

    public void ResetJumps()
    {
        jumpCount = 0;
    }

    public void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance, groundMask);
        if (isGrounded) ResetJumps();
        Debug.DrawRay(transform.position, Vector3.down * groundDistance, isGrounded ? Color.green : Color.red);
    }

    // Dash helper that can be started from a state
    public System.Collections.IEnumerator DashCoroutine(float duration, float speed)
    {
        float t = 0f;
        Vector3 dir = (input != null) ? input.GetMoveDirectionRelativeToCamera(input.GetMovement()) : transform.forward;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
        dir.Normalize();

        while (t < duration)
        {
            controller.Move(dir * speed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
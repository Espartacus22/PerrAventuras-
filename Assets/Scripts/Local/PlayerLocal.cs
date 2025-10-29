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
    [Header("Componentes")]
    public CharacterController controller;
    public PlayerInputHandler input;
    public CharacterType characterData;
    public StateMachine StateMachine;

    [Header("Parámetros")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float gravity = -9.8f;
    public bool isGrounded;

    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public bool canDoubleJump;


    [Header("Habilidades desbloqueadas")]
    public bool dashUnlocked = false;
    public bool doubleJumpUnlocked = false;
    public bool blockUnlocked = false;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        StateMachine = new StateMachine();
    }

    private void Start()
    {
        StateMachine = new StateMachine();
        StateMachine.Initialize(new PlayerMoveState(this));
    }

    private void Update()
    {
        StateMachine.Update();
    }

    // Métodos auxiliares usados por los estados
    public void Move(Vector2 inputMove)
    {
        Vector3 moveDir = input.GetMoveDirectionRelativeToCamera();
        float speed = moveSpeed;

        if (input.isRunning)
            speed *= characterData.runMultiplier;

        if (input.isCrouching)
            speed *= 0.5f;

        controller.Move(moveDir * speed * Time.deltaTime);
    }

    // Dash: puede llamarse desde estado o directamente
    public IEnumerator Dash()
    {
        if (!dashUnlocked && (characterData == null || !characterData.canDash)) yield break;

        float startTime = Time.time;
        Vector3 dashDir = input.GetMoveDirectionRelativeToCamera();
        if (dashDir == Vector3.zero)
            dashDir = transform.forward;

        while (Time.time < startTime + characterData.dashDuration)
        {
            controller.Move(dashDir * characterData.dashSpeed * Time.deltaTime);
            yield return null;
        }
    }

    //Double Jump & Gravity
    public void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        // Solo permite doble salto si está desbloqueado o habilitado
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(characterData.jumpForce * -2f * gravity);
        }
    }

    private void CheckGround()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    // --- Desbloqueo de habilidades ---
    public void EnableDash(bool value) => dashUnlocked = value;
    public void EnableDoubleJump(bool value) => doubleJumpUnlocked = value;
    public void EnableBlock(bool value) => blockUnlocked = value;
}
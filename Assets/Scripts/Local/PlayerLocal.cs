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

    [Header("Parámetros")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float gravity = -9.8f;
    public bool isGrounded;

    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public bool canDoubleJump;

    public StateMachine StateMachine { get; private set; }

    // Habilidades disponibles
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
        Vector3 move = transform.right * inputMove.x + transform.forward * inputMove.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    public void ApplyGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            canDoubleJump = true;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        velocity.y = jumpForce;
    }

    // --- Desbloqueo de habilidades ---
    public void EnableDash(bool value)
    {
        dashUnlocked = value;
        Debug.Log($"Dash {(value ? "habilitado" : "deshabilitado")}");
    }

    public void EnableDoubleJump(bool value)
    {
        doubleJumpUnlocked = value;
        Debug.Log($"Doble salto {(value ? "habilitado" : "deshabilitado")}");
    }

    public void EnableBlock(bool value)
    {
        blockUnlocked = value;
        Debug.Log($"Bloqueo {(value ? "habilitado" : "deshabilitado")}");
    }
}
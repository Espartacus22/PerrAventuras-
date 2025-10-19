using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerLocal: Controla el movimiento, salto, dash y ataque del jugador en modo offline.
/// Compatible con CharacterType (ScriptableObject) y PlayerInputHandler.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerHealthLocal))]
public class PlayerLocal : MonoBehaviour
{
    [Header("Configuración general")]
    public CharacterType characterData;    // ScriptableObject con stats del personaje
    public PlayerInputHandler input;        // Script de input local

    [Header("Componentes")]
    private CharacterController controller;
    private PlayerHealthLocal health;

    [Header("Movimiento")]
    private Vector3 moveDirection;
    private float verticalVelocity;
    private float gravity = -9.81f;
    private bool isGrounded;

    [Header("Dash")]
    private bool isDashing = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealthLocal>();

        if (characterData == null)
            Debug.LogWarning("No hay CharacterType asignado al PlayerLocal.");
        if (input == null)
            Debug.LogWarning("No hay PlayerInputHandler asignado al PlayerLocal.");
    }

    private void Update()
    {
        if (characterData == null || input == null) return;

        HandleMovement();
        HandleJump();
        HandleDash();
        HandleAttack();
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        Vector2 inputDir = input.GetMovement();
        Vector3 move = new Vector3(inputDir.x, 0, inputDir.y);
        move = Camera.main.transform.TransformDirection(move);
        move.y = 0;

        float speed = inputDir.magnitude > 0.5f ? characterData.runMultiplier : characterData.walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        // Gravedad
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (input.GetJump() && isGrounded)
            verticalVelocity = characterData.jumpForce;
    }

    private void HandleDash()
    {
        if (input.GetDash() && !isDashing)
            StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        float dashTime = characterData.dashDuration;
        Vector3 dashDirection = transform.forward * characterData.dashSpeed;

        while (dashTime > 0)
        {
            controller.Move(dashDirection * Time.deltaTime);
            dashTime -= Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void HandleAttack()
    {
        if (input.GetAttack())
        {
            Debug.Log("Ataque ejecutado!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (health != null)
            health.TakeDamage(damage);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerLocal: Controls the player's movement, jumping, dashing, and attacking in offline mode.
/// PlayerLocal: Controla el movimiento, salto, dash y ataque del jugador en modo offline.
/// Supports InputPlayer and CharacterType ScriptableObject.
/// Compatible con InputPlayer y CharacterType ScriptableObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerLocal : MonoBehaviour
{
    [Header("Configuración general")]
    public CharacterType characterData; // ScriptableObject con stats del personaje
    public PlayerInputHandler input;            // Script de input local

    [Header("Componentes")]
    private CharacterController controller;
    private PlayerHealthLocal health;

    [Header("Movimiento")]
    private Vector3 moveDirection;
    private float gravity = -9.81f;
    private float verticalVelocity;
    private bool isGrounded;

    [Header("Dash")]
    private bool isDashing = false;
    private float dashTimer = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealthLocal>();

        if (characterData == null)
            Debug.LogWarning("No hay CharacterType asignado al PlayerLocal.");
    }

    private void Update()
    {
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

        float speed = inputDir.magnitude > 0.5f ? characterData.RunMultiplier : characterData.WalkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        // Gravedad y grounded check
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (input.GetJump() && isGrounded)
        {
            verticalVelocity = characterData.JumpForce;
        }
    }

    private void HandleDash()
    {
        if (input.GetDash() && !isDashing)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        float dashTime = characterData.DashDuration;
        Vector3 dashDirection = transform.forward * characterData.DashSpeed;

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
            // Temporary: melee attack / Provisorio: ataque cuerpo a cuerpo
            Debug.Log("Ataque ejecutado");
        }
    }

    public void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }
}

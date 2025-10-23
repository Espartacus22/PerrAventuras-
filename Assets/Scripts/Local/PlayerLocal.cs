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
    [Header("Datos del personaje")]
    public CharacterType characterData;            // ScriptableObject con stats
    public PlayerInputHandler input;               // Script de input (objeto en escena)

    [Header("Componentes / Prefabs")]
    public GameObject projectilePrefab;            // Prefab (opcional) para ataques a distancia
    public Transform projectileSpawnPoint;

    // Internos
    private CharacterController controller;
    private PlayerHealthLocal health;

    // Movimiento
    private Vector3 velocity;
    private float verticalVelocity;
    private bool isGrounded;

    // Dash
    private bool isDashing = false;

    // Crouch (simple: ajusta height del CharacterController)
    private float originalHeight;
    private Vector3 originalCenter;

    // Animator opcional
    public Animator animator;

    [Header("Habilidades desbloqueables")]
    public bool canDash;
    public bool canDoubleJump;
    public bool canBlock;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealthLocal>();

        if (characterData == null) Debug.LogWarning("CharacterType no asignado en PlayerLocal.");
        if (input == null) Debug.LogWarning("PlayerInputHandler no asignado en PlayerLocal.");

        originalHeight = controller.height;
        originalCenter = controller.center;
    }

    void Update()
    {
        if (characterData == null || input == null) return;

        HandleMovement();
        HandleJump();
        HandleDash();
        HandleCrouch();
        HandleAttacks();
        ApplyGravity();
        UpdateAnimator();
    }

    #region Movimiento y gravedad
    private void HandleMovement()
    {
        if (isDashing) return;

        Vector2 move = input.GetMovement();
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 desiredMove = forward * move.y + right * move.x;

        // velocidad: caminar, correr
        float baseSpeed = characterData.walkSpeed;
        if (input.GetRun())
            baseSpeed *= characterData.runMultiplier;

        controller.Move(desiredMove * baseSpeed * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        // Lanza un rayo corto hacia abajo desde el centro del personaje
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
    private void ApplyGravity()
    {
        
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    #endregion
    
   


    #region Saltar
    private void HandleJump()
    {
        if (input.GetJump() && IsGrounded())
        {
            verticalVelocity = characterData.jumpForce;
        }
    }
    #endregion

    #region Dash
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
        float time = characterData.dashDuration;
        Vector3 dir = transform.forward * characterData.dashSpeed;

        while (time > 0f)
        {
            controller.Move(dir * Time.deltaTime);
            time -= Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
    #endregion

    #region Crouch
    private void HandleCrouch()
    {
        if (input.GetCrouch())
        {
            controller.height = Mathf.Lerp(controller.height, 1.0f, Time.deltaTime * 8f);
            controller.center = new Vector3(0, 0.5f, 0);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, 2.0f, Time.deltaTime * 8f);
            controller.center = new Vector3(0, 1.0f, 0);
        }
    }
    #endregion

    #region Ataques
    private void HandleAttacks()
    {
        // Melee
        if (input.GetAttackMelee())
        {
            DoMeleeAttack();
        }

        // Ranged
        if (input.GetAttackRanged())
        {
            DoRangedAttack();
        }
    }

    private void DoMeleeAttack()
    {
        // Placeholder: aplicar daño a enemigos cercanos (implementá overlap sphere + layerMask)
        Debug.Log("Melee attack!");
        if (animator != null) animator.SetTrigger("Melee");
    }

    private void DoRangedAttack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;
        GameObject p = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        ProjectileLocal proj = p.GetComponent<ProjectileLocal>();
        if (proj != null)
            proj.SetDirection(transform.forward);
        if (animator != null) animator.SetTrigger("Ranged");
    }
    #endregion

    public void TakeDamage(int damage)
    {
        if (health != null) health.TakeDamage(damage);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        Vector2 move = input.GetMovement();
        float speed = move.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", isGrounded);
        // otros parámetros según tus animaciones
    }

    // Activa o desactiva el dash
    public void EnableDash(bool enabled)
    {
        canDash = enabled;
        Debug.Log(enabled ? "Dash habilitado" : "Dash deshabilitado");
    }

    // Activa o desactiva el doble salto
    public void EnableDoubleJump(bool enabled)
    {
        canDoubleJump = enabled;
        Debug.Log(enabled ? "Doble salto habilitado" : "Doble salto deshabilitado");
    }

    // Activa o desactiva el bloqueo/escudo
    public void EnableBlock(bool enabled)
    {
        canBlock = enabled;
        Debug.Log(enabled ? "Bloqueo habilitado" : "Bloqueo deshabilitado");
    }
}
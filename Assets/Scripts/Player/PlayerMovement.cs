using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Datos del personaje")]
    public CharacterType characterData;

    [Header("Combate")]
    public int selectedMeleeIndex = 0;
    public int selectedRangedIndex = 0;

    [Header("Saltos")]
    [SerializeField] private bool hasDoubleJump = false;

    [Header("Ground")]
    public LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;

    private int currentComboIndex = -1;
    private float comboResetTimer = 0f;
    private float comboResetDelay = 1f;
    private float lastAttackTime;

    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    private CapsuleCollider capsule;
    private PlayerInputHandler inputHandler;

    public bool isGrounded;
    private bool isDashing;
    private bool isCrouching;
    private float originalHeight;
    private Vector3 originalCenter;

    private int jumpCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<PlayerInputHandler>();

        if (rb == null)
        {
            Debug.LogError($"[{name}] PlayerMovement: Falta Rigidbody.");
            enabled = false;
            return;
        }

        if (capsule == null)
        {
            Debug.LogError($"[{name}] PlayerMovement: Falta CapsuleCollider.");
            enabled = false;
            return;
        }

        if (inputHandler == null)
        {
            Debug.LogError($"[{name}] PlayerMovement: Falta PlayerInputHandler.");
            enabled = false;
            return;
        }

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        rb.freezeRotation = true;

        // Arranca sin doble salto.
        // Después lo podés habilitar desde NPC1 o desde otro sistema de unlocks.
        hasDoubleJump = false;
    }

    private void Update()
    {
        if (characterData == null) return;

        UpdateGroundCheck();

        HandleMovement();
        HandleJump();
        HandleDash();
        HandleCrouch();

        RotateTowardsMouse();

        if (inputHandler.MeleePressed)
        {
            if (currentComboIndex == -1)
                currentComboIndex = selectedMeleeIndex;

            TryMeleeAttack();
        }

        if (inputHandler.RangedPressed)
        {
            TryRangedAttack();
        }

        if (Time.time > comboResetTimer)
            currentComboIndex = -1;

        inputHandler.ConsumeFrameInput();
    }

    private void UpdateGroundCheck()
    {
        bool wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        }
        else
        {
            // Fallback por si todavía no asignaste un groundCheck en inspector
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float distance = (capsule.height * 0.5f) + 0.2f;
            isGrounded = Physics.Raycast(origin, Vector3.down, distance, groundMask);
        }

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

        if (input.sqrMagnitude > 0.01f)
        {
            Transform cam = Camera.main != null ? Camera.main.transform : null;
            Vector3 moveDir = input.normalized;

            if (cam != null)
            {
                Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
                moveDir = (camForward * input.z + camRight * input.x).normalized;
            }

            float currentSpeed = characterData.walkSpeed;

            if (isCrouching)
                currentSpeed *= characterData.crouchMultiplier;
            else if (inputHandler.RunHeld)
                currentSpeed *= characterData.runMultiplier;

            rb.linearVelocity = new Vector3(
                moveDir.x * currentSpeed,
                rb.linearVelocity.y,
                moveDir.z * currentSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void RotateTowardsMouse()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    characterData.rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void HandleJump()
    {
        if (!inputHandler.JumpPressed || isDashing)
            return;

        int maxJumps = hasDoubleJump ? 2 : 1;

        if (jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                characterData.jumpForce,
                rb.linearVelocity.z
            );

            jumpCount++;
        }
    }

    private void HandleDash()
    {
        if (!inputHandler.DashPressed || isDashing)
            return;

        StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 dashDir = input.normalized;

        if (cam != null)
        {
            Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
            dashDir = (camForward * input.z + camRight * input.x).normalized;
        }

        if (dashDir == Vector3.zero)
            dashDir = transform.forward;

        float startTime = Time.time;

        while (Time.time < startTime + characterData.dashDuration)
        {
            rb.linearVelocity = dashDir * characterData.dashSpeed;
            yield return null;
        }

        isDashing = false;
    }

    private void HandleCrouch()
    {
        if (inputHandler.CrouchPressed && !isCrouching)
        {
            isCrouching = true;

            float newHeight = originalHeight * characterData.crouchHeight;
            capsule.height = newHeight;

            float heightDelta = (originalHeight - newHeight) * 0.5f;
            capsule.center = originalCenter - new Vector3(0f, heightDelta, 0f);
        }
        else if (inputHandler.CrouchReleased && isCrouching)
        {
            isCrouching = false;
            capsule.height = originalHeight;
            capsule.center = originalCenter;
        }
    }

    private void TryMeleeAttack()
    {
        if (characterData == null || characterData.meleeAttacks == null) return;
        if (currentComboIndex < 0 || currentComboIndex >= characterData.meleeAttacks.Count) return;

        var attack = characterData.meleeAttacks[currentComboIndex];

        if (Time.time < lastAttackTime + attack.cooldown) return;
        lastAttackTime = Time.time;

        var level = GetComponent<PlayerLevel>();
        if (level != null && level.currentLevel < attack.requiredLevel)
        {
            Debug.Log("Ataque no desbloqueado todavía");
            return;
        }

        if (animator != null && attack.animation != null)
            animator.Play(attack.animation.name);

        if (audioSource != null && attack.sound != null)
            audioSource.PlayOneShot(attack.sound);

        if (attack.impactEffectPrefab != null)
            Instantiate(attack.impactEffectPrefab, transform.position + transform.forward, transform.rotation);

        Collider[] hitEnemies = Physics.OverlapSphere(
            transform.position + transform.forward * attack.range * 0.5f,
            attack.range * 0.5f
        );

        foreach (Collider col in hitEnemies)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStats enemy = col.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    int damageDealt = Mathf.RoundToInt(attack.damage);
                    enemy.TakeDamage(damageDealt);
                }
            }
        }

        if (attack.canChainCombo && characterData.meleeAttacks.Count > attack.nextComboIndex)
        {
            currentComboIndex = attack.nextComboIndex;
            comboResetTimer = Time.time + comboResetDelay;
        }
        else
        {
            currentComboIndex = -1;
        }
    }

    private void TryRangedAttack()
    {
        if (characterData == null || characterData.rangedAttacks == null) return;
        if (selectedRangedIndex < 0 || selectedRangedIndex >= characterData.rangedAttacks.Count) return;

        var attack = characterData.rangedAttacks[selectedRangedIndex];

        if (Time.time < lastAttackTime + attack.cooldown) return;
        lastAttackTime = Time.time;

        var level = GetComponent<PlayerLevel>();
        if (level != null && level.currentLevel < attack.requiredLevel)
        {
            Debug.Log("Ataque a distancia no desbloqueado todavía");
            return;
        }

        if (animator != null && attack.animation != null)
            animator.Play(attack.animation.name);

        if (audioSource != null && attack.sound != null)
            audioSource.PlayOneShot(attack.sound);

        Vector3 shootDirection = transform.forward;

        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                shootDirection = (hit.point - transform.position).normalized;
            }
        }

        if (shootDirection.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(shootDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                characterData.rotationSpeed * Time.deltaTime
            );
        }

        if (attack.projectilePrefab == null)
        {
            Debug.LogWarning("No hay prefab asignado para el ataque a distancia.");
            return;
        }

        GameObject projectile = Instantiate(
            attack.projectilePrefab,
            transform.position + shootDirection,
            Quaternion.LookRotation(shootDirection)
        );

        ProjectileBehavior pb = projectile.GetComponent<ProjectileBehavior>();
        if (pb != null)
        {
            pb.SetRange(attack.range);
            pb.SetDamage(attack.damage);
        }
    }

    public void UnlockDoubleJump()
    {
        hasDoubleJump = true;
        Debug.Log($"{name}: Doble salto desbloqueado.");
    }

    public void LockDoubleJump()
    {
        hasDoubleJump = false;
        Debug.Log($"{name}: Doble salto bloqueado.");
    }

    public bool HasDoubleJump()
    {
        return hasDoubleJump;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
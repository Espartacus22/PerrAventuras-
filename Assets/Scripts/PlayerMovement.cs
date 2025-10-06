using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterType characterData;

    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    private CapsuleCollider capsule;

    private bool isGrounded;
    private bool isDashing;
    private bool isCrouching;
    private float originalHeight;
    private Vector3 originalCenter;

    private int jumpCount;
    private float lastShiftTime;
    private float doubleTapThreshold = 0.3f;

    private int currentComboIndex = 0;
    private float comboResetTimer = 0f;
    private float comboResetDelay = 1f;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            originalHeight = capsule.height;
            originalCenter = capsule.center;
        }

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleCrouch();

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Bot�n izquierdo presionado");
            TryMeleeAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Bot�n derecho presionado");
            TryRangedAttack(0);
        }

        if (Time.time > comboResetTimer)
            currentComboIndex = 0;
    }

    void HandleMovement()
    {
        if (isDashing) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDir.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, characterData.rotationSpeed * Time.deltaTime);

            float currentSpeed = characterData.walkSpeed;
            if (isCrouching)
                currentSpeed *= characterData.crouchMultiplier;
            else if (Input.GetKey(KeyCode.LeftShift))
                currentSpeed *= characterData.runMultiplier;

            Vector3 velocity = moveDir * currentSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && !isDashing)
        {
            int maxJumps = characterData.dobleSalto ? 2 : 1;
            if (jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, characterData.jumpForce, rb.linearVelocity.z);
                jumpCount++;
            }
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            float timeSinceLastTap = Time.time - lastShiftTime;
            if (timeSinceLastTap <= doubleTapThreshold && !isDashing)
                StartCoroutine(Dash());
            lastShiftTime = Time.time;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (inputDirection == Vector3.zero) inputDirection = transform.forward;

        float startTime = Time.time;

        while (Time.time < startTime + characterData.dashDuration)
        {
            rb.linearVelocity = inputDirection * characterData.dashSpeed;
            yield return null;
        }

        isDashing = false;
    }

    void HandleCrouch()
    {
        bool crouchPressed = Input.GetKeyDown(KeyCode.LeftControl);
        bool crouchReleased = Input.GetKeyUp(KeyCode.LeftControl);

        if (capsule != null)
        {
            if (crouchPressed && !isCrouching)
            {
                isCrouching = true;
                capsule.height = originalHeight * characterData.crouchHeight;
                capsule.center = new Vector3(originalCenter.x, originalCenter.y * characterData.crouchHeight, originalCenter.z);
            }
            else if (crouchReleased && isCrouching)
            {
                isCrouching = false;
                capsule.height = originalHeight;
                capsule.center = originalCenter;
            }
        }
    }

    void TryMeleeAttack()
    {
        if (characterData == null || characterData.meleeAttacks.Count == 0) return;

        var attack = characterData.meleeAttacks[currentComboIndex];
        if (Time.time < lastAttackTime + attack.cooldown) return;

        lastAttackTime = Time.time;

        if (attack.animation != null) animator.Play(attack.animation.name);
        if (attack.sound != null) audioSource.PlayOneShot(attack.sound);
        if (attack.impactEffectPrefab != null)
            Instantiate(attack.impactEffectPrefab, transform.position + transform.forward, transform.rotation);

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * attack.range * 0.5f, attack.range * 0.5f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log($"Golpe� a: {enemy.name} con {attack.damage} de da�o.");
                // enemy.GetComponent<EnemyHealth>()?.TakeDamage(attack.damage);
            }
        }

        if (attack.canChainCombo)
        {
            currentComboIndex = attack.nextComboIndex;
            comboResetTimer = Time.time + comboResetDelay;
        }
        else
        {
            currentComboIndex = 0;
        }
    }

    void TryRangedAttack(int index)
    {
        if (characterData == null || index >= characterData.rangedAttacks.Count) return;

        var attack = characterData.rangedAttacks[index];
        if (Time.time < lastAttackTime + attack.cooldown) return;

        lastAttackTime = Time.time;

        if (attack.animation != null) animator.Play(attack.animation.name);
        if (attack.sound != null) audioSource.PlayOneShot(attack.sound);

        if (attack.projectilePrefab != null)
        {
            Debug.Log($"Instanciando proyectil: {attack.attackName}");
            GameObject projectile = Instantiate(
                attack.projectilePrefab,
                transform.position + transform.forward,
                transform.rotation
            );

            ProjectileBehavior pb = projectile.GetComponent<ProjectileBehavior>();
            if (pb != null)
            {
                pb.SetRange(attack.range);
                pb.SetDamage(attack.damage);
            }
            else
            {
                Debug.LogWarning("El proyectil no tiene el script ProjectileBehavior.");
            }
        }
        else
        {
            Debug.LogWarning("No hay prefab asignado para el ataque a distancia.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (characterData != null && characterData.meleeAttacks.Count > currentComboIndex)
        {
            var attack = characterData.meleeAttacks[currentComboIndex];
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attack.range * 0.5f, attack.range * 0.5f);
        }
    }
}
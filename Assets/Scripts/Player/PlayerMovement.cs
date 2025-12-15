using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
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

    private int currentComboIndex = -1;
    private float comboResetTimer = 0f;
    private float comboResetDelay = 1f;
    private float lastAttackTime;

    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    private CapsuleCollider capsule;

    public bool isGrounded;
    private bool isDashing;
    private bool isCrouching;
    private float originalHeight;
    private Vector3 originalCenter;

    private int jumpCount;
    private float lastShiftTime;
    private float doubleTapThreshold = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

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

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        rb.freezeRotation = true;

        // Arranca sin doble salto (lo habilita PlayerLevel al subir a nivel 1)
        hasDoubleJump = false;
    }

    void Update()
    {
        if (characterData == null) return;

        HandleMovement();
        HandleJump();
        HandleDash();
        HandleCrouch();

        RotateTowardsMouse();

        if (Input.GetMouseButtonDown(0))
        {
            if (currentComboIndex == -1)
                currentComboIndex = selectedMeleeIndex;

            TryMeleeAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryRangedAttack();
        }

        if (Time.time > comboResetTimer)
            currentComboIndex = -1;
    }

    void HandleMovement()
    {
        if (isDashing) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(moveX, 0, moveZ);

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
            else if (Input.GetKey(KeyCode.LeftShift))
                currentSpeed *= characterData.runMultiplier;

            rb.linearVelocity = new Vector3(moveDir.x * currentSpeed, rb.linearVelocity.y, moveDir.z * currentSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void RotateTowardsMouse()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, characterData.rotationSpeed * Time.deltaTime);
            }
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && !isDashing)
        {
            int maxJumps = hasDoubleJump ? 2 : 1;

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

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(moveX, 0, moveZ);

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

    void HandleCrouch()
    {
        bool crouchPressed = Input.GetKeyDown(KeyCode.LeftControl);
        bool crouchReleased = Input.GetKeyUp(KeyCode.LeftControl);

        if (crouchPressed && !isCrouching)
        {
            isCrouching = true;

            float newHeight = originalHeight * characterData.crouchHeight;
            capsule.height = newHeight;

            float heightDelta = (originalHeight - newHeight) * 0.5f;
            capsule.center = originalCenter - new Vector3(0f, heightDelta, 0f);
        }
        else if (crouchReleased && isCrouching)
        {
            isCrouching = false;
            capsule.height = originalHeight;
            capsule.center = originalCenter;
        }
    }

    void TryMeleeAttack()
    {
        if (characterData == null || characterData.meleeAttacks == null) return;
        if (currentComboIndex < 0 || currentComboIndex >= characterData.meleeAttacks.Count) return;

        var attack = characterData.meleeAttacks[currentComboIndex];

        if (Time.time < lastAttackTime + attack.cooldown) return;
        lastAttackTime = Time.time;

        // Animación y sonido (seguros)
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

    void TryRangedAttack()
    {
        if (characterData == null || characterData.rangedAttacks == null) return;
        if (selectedRangedIndex < 0 || selectedRangedIndex >= characterData.rangedAttacks.Count) return;

        var attack = characterData.rangedAttacks[selectedRangedIndex];

        if (Time.time < lastAttackTime + attack.cooldown) return;
        lastAttackTime = Time.time;

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

        // Rotar hacia dirección de disparo (suave)
        if (shootDirection.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(shootDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, characterData.rotationSpeed * Time.deltaTime);
        }

        if (attack.projectilePrefab == null)
        {
            Debug.LogWarning("No hay prefab asignado para el ataque a distancia.");
            return;
        }

        GameObject projectile = Instantiate(
            attack.projectilePrefab,
            transform.position + shootDirection, // spawn adelante
            Quaternion.LookRotation(shootDirection)
        );

        ProjectileBehavior pb = projectile.GetComponent<ProjectileBehavior>();
        if (pb != null)
        {
            pb.SetRange(attack.range);
            pb.SetDamage(attack.damage);
        }
    }

    public void EnableDoubleJump(bool enabled) => hasDoubleJump = enabled;
    public void UnlockDoubleJump() => hasDoubleJump = true;

    // ---------- GROUND / COLISIONES ----------

    void OnCollisionEnter(Collision collision) => CheckGroundCollision(collision);
    void OnCollisionStay(Collision collision) => CheckGroundCollision(collision);

    void OnCollisionExit(Collision collision)
    {
        if (collision != null && collision.gameObject != null && IsGroundLayer(collision.gameObject.layer))
            isGrounded = false;
    }

    bool IsGroundLayer(int layer) => (groundMask & (1 << layer)) != 0;

    void CheckGroundCollision(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (!IsGroundLayer(collision.gameObject.layer)) return;
        if (rb == null) return;

        // Si estoy subiendo, no resetear saltos
        if (rb.linearVelocity.y > 0.01f) return;

        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y >= 0.85f)
            {
                isGrounded = true;
                jumpCount = 0;
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (characterData != null && characterData.meleeAttacks != null && characterData.meleeAttacks.Count > selectedMeleeIndex)
        {
            var attack = characterData.meleeAttacks[selectedMeleeIndex];
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attack.range * 0.5f, attack.range * 0.5f);
        }
    }
}
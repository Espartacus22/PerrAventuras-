using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 6f;
    public float runMultiplier = 1.5f; // Aumenta velocidad al correr
    public float crouchMultiplier = 0.5f; // Reduce velocidad al agacharse
    public float rotationSpeed = 10f;

    [Header("Salto")]
    public float jumpForce = 8f;
    public float groundRayLength = 0.6f;
    public LayerMask groundMask;
    public int maxJumps = 2;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Agacharse")]
    public float crouchHeight = 0.5f; // Escala de Y cuando se agacha
    private float originalHeight;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isDashing;
    private int jumpCount;
    private bool isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        originalHeight = transform.localScale.y; // Guardamos altura original
    }

    void Update()
    {
        // Verificar suelo
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundRayLength, groundMask);
        if (isGrounded) jumpCount = 0;

        // Movimiento
        if (!isDashing)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

            if (moveDir.magnitude >= 0.1f)
            {
                // Rotación
                Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Velocidad base
                float currentSpeed = walkSpeed;

                // Si está corriendo
                if (Input.GetKey(KeyCode.LeftShift))
                    currentSpeed *= runMultiplier;

                // Si está agachado
                if (isCrouching)
                    currentSpeed *= crouchMultiplier;

                // Movimiento final
                Vector3 velocity = moveDir * currentSpeed;
                rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        // Saltar / Doble salto
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps && !isDashing)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpCount++;
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.E) && !isDashing) // cambié a tecla E (Shift ya es correr)
        {
            StartCoroutine(Dash());
        }

        // Agacharse
        if (Input.GetKeyDown(KeyCode.C) && !isCrouching)
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        }
        else if (Input.GetKeyUp(KeyCode.C) && isCrouching)
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, originalHeight, transform.localScale.z);
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        Vector3 dashDirection = transform.forward;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }

        isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRayLength);
    }
}

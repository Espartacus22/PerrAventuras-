using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2.5f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.18f;

    [Header("Salto")]
    public float jumpForce = 6f;
    public int maxJumps = 2;
    public float gravity = -20f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundMask;

    [Header("Opciones")]
    public bool useLocalInput = true; // true = usa PlayerInputHandler, false = usa input de red (ApplyNetworkInput)
    public float networkInputTimeout = 0.5f; // tiempo sin recibir input de red para volver a local

    // estado
    CharacterController controller;
    PlayerInputHandler localInput;

    // estado runtime
    Vector3 velocity;
    bool isGrounded;
    int jumpCount = 0;
    bool isDashing = false;
    float dashTimer = 0f;

    // datos de input (comunes)
    struct InputState { public float h; public float v; public bool run; public bool crouch; public bool jump; public bool dash; }
    InputState currentInput;           // input a usar en cada frame
    InputState networkInputBuffer;     // último input recibido desde la red
    bool networkInputActive = false;
    float lastNetworkInputTime = -999f;

    private float originalHeight;
    public float crouchHeight = 1.0f; // altura al agacharse

    void Start()
    {
        controller = GetComponent<CharacterController>();
        localInput = GetComponent<PlayerInputHandler>();
        originalHeight = controller.height;

        // seguridad: si no asignaste groundCheck, crea uno temporal
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, 0.1f, 0);
            groundCheck = go.transform;
        }
    }

    void HandleCrouch()
    {
        if (currentInput.crouch)
        {
            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2f, 0);
        }
        else
        {
            controller.height = originalHeight;
            controller.center = new Vector3(0, originalHeight / 2f, 0);
        }
    }

    void Update()
    {
        // decidir qué input usar
        if (useLocalInput && localInput != null)
        {
            ReadLocalInputToCurrent();
        }
        else if (networkInputActive && Time.time - lastNetworkInputTime <= networkInputTimeout)
        {
            currentInput = networkInputBuffer;
        }
        else
        {
            // fallback: intentar local
            if (localInput != null) ReadLocalInputToCurrent();
        }

        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }

        // === Movimiento horizontal relativo a la cámara ===
        Vector3 move = Vector3.zero;

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            move = camRight * currentInput.h + camForward * currentInput.v;
        }
        else
        {
            // fallback si no hay cámara
            move = transform.right * currentInput.h + transform.forward * currentInput.v;
        }

        float speed = currentInput.run ? runSpeed : walkSpeed;
        if (currentInput.crouch) speed = crouchSpeed;

        // dash
        if (currentInput.dash && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }

        if (isDashing)
        {
            controller.Move(move.normalized * dashSpeed * Time.deltaTime);
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }
        else
        {
            controller.Move(move * speed * Time.deltaTime);
        }

        // === Rotación del player hacia la dirección de movimiento ===
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // salto
        if (currentInput.jump)
        {
            if (isGrounded || jumpCount < maxJumps)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                jumpCount++;
            }

            if (useLocalInput && localInput != null)
            {
                ReadLocalInputToCurrent();
            }
        }

        // gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ReadLocalInputToCurrent()
    {
        currentInput.h = localInput.moveInput.x;   // Horizontal (A/D, stick)
        currentInput.v = localInput.moveInput.y;   // Vertical (W/S, stick)
        currentInput.run = localInput.run;
        currentInput.crouch = localInput.crouch;
        currentInput.jump = localInput.jump;
        currentInput.dash = localInput.dash;
    }

    public void ApplyNetworkInput(NetworkInputData data)
    {
        networkInputBuffer.h = data.move.x;
        networkInputBuffer.v = data.move.y;
        networkInputBuffer.run = data.run;
        networkInputBuffer.crouch = data.crouch;
        networkInputBuffer.jump = data.jump;
        networkInputBuffer.dash = data.dash;

        networkInputActive = true;
        lastNetworkInputTime = Time.time;
    }

    public void SetUseLocalInput(bool useLocal)
    {
        useLocalInput = useLocal;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}

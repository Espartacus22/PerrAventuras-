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

    [Header("Ground")]
    public LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 1.2f;
    [SerializeField] private float groundRayOffset = 0.1f;

    [Header("Saltos")]
    [SerializeField] private bool hasDoubleJump = false;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler inputHandler;

    private bool isGrounded;
    private bool isCrouching;
    private bool isDashing;
    private int jumpCount;

    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 dashDirection;

    public PStateMachine StateMachine { get; private set; }

    public PIdleState IdleState { get; private set; }
    public PMoveState MoveState { get; private set; }
    public PRunState RunState { get; private set; }
    public PJumpState JumpState { get; private set; }
    public PDashState DashState { get; private set; }
    public PCrouchState CrouchState { get; private set; }

    public PlayerInputHandler InputHandler => inputHandler;
    public CharacterType CharacterData => characterData;
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => rb.linearVelocity.y;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        inputHandler = GetComponent<PlayerInputHandler>();

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        rb.freezeRotation = true;

        StateMachine = new PStateMachine();
        IdleState = new PIdleState(this, StateMachine);
        MoveState = new PMoveState(this, StateMachine);
        RunState = new PRunState(this, StateMachine);
        JumpState = new PJumpState(this, StateMachine);
        DashState = new PDashState(this, StateMachine);
        CrouchState = new PCrouchState(this, StateMachine);

        hasDoubleJump = false;
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (characterData == null) return;

        UpdateGroundCheck();
        RotateTowardsMouse();

        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        if (characterData == null) return;

        StateMachine.CurrentState.PhysicsUpdate();
    }

    private void UpdateGroundCheck()
    {
        bool wasGrounded = isGrounded;

        Vector3 origin = transform.position + Vector3.up * groundRayOffset;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    public bool HasMovementInput()
    {
        return inputHandler.MoveInput.sqrMagnitude > 0.01f;
    }

    public void Move(bool running)
    {
        if (isDashing) return;

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

        if (input.sqrMagnitude <= 0.01f)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 moveDir = GetCameraRelativeDirection(input);
        float speed = characterData.walkSpeed;

        if (isCrouching)
            speed *= characterData.crouchMultiplier;
        else if (running)
            speed *= characterData.runMultiplier;

        rb.linearVelocity = new Vector3(
            moveDir.x * speed,
            rb.linearVelocity.y,
            moveDir.z * speed
        );
    }

    public void MoveInAir()
    {
        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

        if (input.sqrMagnitude <= 0.01f) return;

        Vector3 moveDir = GetCameraRelativeDirection(input);
        float speed = characterData.walkSpeed * 0.8f;

        rb.linearVelocity = new Vector3(
            moveDir.x * speed,
            rb.linearVelocity.y,
            moveDir.z * speed
        );
    }

    public void MoveCrouched()
    {
        Move(false);
    }

    public void StopHorizontalMovement()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    public void Jump()
    {
        int maxJumps = hasDoubleJump ? 2 : 1;

        if (jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                characterData.jumpForce,
                rb.linearVelocity.z
            );

            jumpCount++;
            Debug.Log($"Salto ejecutado. jumpCount={jumpCount}, maxJumps={maxJumps}, hasDoubleJump={hasDoubleJump}");

        }
    }

    public void BeginDash()
    {
        isDashing = true;

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

        dashDirection = GetCameraRelativeDirection(input);
        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;
    }

    public void DashMove()
    {
        rb.linearVelocity = new Vector3(
            dashDirection.x * characterData.dashSpeed,
            rb.linearVelocity.y,
            dashDirection.z * characterData.dashSpeed
        );
    }

    public void EndDash()
    {
        isDashing = false;
    }

    public void StartCrouch()
    {
        if (isCrouching) return;

        isCrouching = true;

        float newHeight = originalHeight * characterData.crouchHeight;
        capsule.height = newHeight;

        float heightDelta = (originalHeight - newHeight) * 0.5f;
        capsule.center = originalCenter - new Vector3(0f, heightDelta, 0f);
    }

    public void StopCrouch()
    {
        if (!isCrouching) return;

        isCrouching = false;
        capsule.height = originalHeight;
        capsule.center = originalCenter;
    }

    private Vector3 GetCameraRelativeDirection(Vector3 input)
    {
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 moveDir = input.normalized;

        if (cam != null)
        {
            Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
            moveDir = (camForward * input.z + camRight * input.x).normalized;
        }

        return moveDir;
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
        Gizmos.color = isGrounded ? Color.green : Color.red;

        Vector3 origin = transform.position + Vector3.up * groundRayOffset;
        Vector3 end = origin + Vector3.down * groundCheckDistance;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
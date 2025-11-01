using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Transform))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction shootAction;

    public Transform cameraTransform; // asignar Main Camera en el inspector si querés

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogWarning("PlayerInput no encontrado en PlayerInputHandler, usando Input.GetAxis fallback.");
            return;
        }

        // Nombres deben coincidir con tu InputActions asset
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
        shootAction = playerInput.actions["Shoot"];
    }

    // --- Lecturas de input (API pública, usar desde estados) ---
    public Vector2 GetMovement()
    {
        if (playerInput != null && moveAction != null) return moveAction.ReadValue<Vector2>();
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public bool GetJump()
    {
        if (playerInput != null && jumpAction != null) return jumpAction.triggered;
        return Input.GetButtonDown("Jump");
    }

    public bool GetDash()
    {
        if (playerInput != null && dashAction != null) return dashAction.triggered;
        return Input.GetKeyDown(KeyCode.LeftShift);
    }

    public bool GetRun()
    {
        if (playerInput != null && runAction != null) return runAction.IsPressed();
        return Input.GetKey(KeyCode.LeftAlt);
    }

    public bool GetCrouch()
    {
        if (playerInput != null && crouchAction != null) return crouchAction.IsPressed();
        return Input.GetKey(KeyCode.LeftControl);
    }

    public bool GetShoot()
    {
        if (playerInput != null && shootAction != null) return shootAction.triggered;
        return Input.GetMouseButtonDown(0);
    }

    // Helper que ya pediste: devuelve vector world-aligned respecto a la cámara
    public Vector3 GetMoveDirectionRelativeToCamera(Vector2 moveInput)
    {
        Transform cam = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (cam == null)
        {
            // fallback: mover relativo al transform del jugador (caller puede añadir transform.right/forward)
            return new Vector3(moveInput.x, 0f, moveInput.y);
        }

        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cam.right; right.y = 0f; right.Normalize();
        return right * moveInput.x + forward * moveInput.y;
    }
}
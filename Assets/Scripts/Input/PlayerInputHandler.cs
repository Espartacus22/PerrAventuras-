using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public InputsPlayer inputs;           // ScriptableObject de teclas
    public Transform cameraTransform;     // Asignar Main Camera

    // Valores públicos leídos cada frame
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool dashPressed;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isCrouching;

    void Update()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        isRunning = inputs != null && Input.GetKey(inputs.runKey);
        isCrouching = inputs != null && Input.GetKey(inputs.crouchKey);
        dashPressed = inputs != null && Input.GetKeyDown(inputs.dashKey);
        jumpPressed = inputs != null && Input.GetKeyDown(inputs.jumpKey);
    }

    // Devuelve dirección en mundo relativa a la cámara (y plana en Y)
    public Vector3 GetMoveDirectionRelativeToCamera()
    {
        if (cameraTransform == null) return Vector3.zero;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return (forward * moveInput.y + right * moveInput.x).normalized;
    }
}
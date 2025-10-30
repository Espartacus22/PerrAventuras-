using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Transform))]
public class PlayerInputHandler : MonoBehaviour
{
    public InputsPlayer inputsAsset;   // opcional ScriptableObject con keybinds
    public Transform cameraTransform;  // asignar Main Camera (opcional)

    void Update() // actualiza cada frame
    {
        // nothing stored here intentionally — usamos métodos públicos para leer on-demand
    }

    // returns movement vector (x = horizontal A/D, y = vertical W/S)
    public Vector2 GetMovement()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // true on jump press (edge)
    public bool GetJump()
    {
        if (inputsAsset != null) return Input.GetKeyDown(inputsAsset.jumpKey);
        return Input.GetButtonDown("Jump");
    }

    // true on dash press (edge)
    public bool GetDash()
    {
        if (inputsAsset != null) return Input.GetKeyDown(inputsAsset.dashKey);
        return Input.GetKeyDown(KeyCode.LeftShift);
    }

    // convenience
    public bool GetRun()
    {
        if (inputsAsset != null) return Input.GetKey(inputsAsset.runKey);
        return Input.GetKey(KeyCode.LeftAlt);
    }

    public bool GetCrouch()
    {
        if (inputsAsset != null) return Input.GetKey(inputsAsset.crouchKey);
        return Input.GetKey(KeyCode.LeftControl);
    }

    // For camera-aligned movement (optional helper)
    public Vector3 GetMoveDirectionRelativeToCamera(Vector2 moveInput)
    {
        Transform cam = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (cam == null) return new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 forward = cam.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        return right * moveInput.x + forward * moveInput.y;
    }
}
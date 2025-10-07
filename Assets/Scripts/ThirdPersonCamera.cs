using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Asigna tu jugador

    [Header("Distancia y Altura")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 8f;
    public Vector3 offset = Vector3.zero;

    [Header("Sensibilidad")]
    public float sensitivity = 2f;
    public float stickSensitivity = 100f;

    [Header("Ángulo vertical")]
    public float minYAngle = -20f;
    public float maxYAngle = 60f;

    [Header("Suavizado")]
    public float rotationSmoothTime = 0.05f;

    private float currentX = 0f;
    private float currentY = 10f;
    private float currentVelocityX;
    private float currentVelocityY;

    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Enable();
    }

    void LateUpdate()
    {
        if (!target) return;

        // === Entrada del Input System (mouse y stick derecho) ===
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float targetX = currentX + lookInput.x * sensitivity;
        float targetY = currentY - lookInput.y * sensitivity;

        // Suavizado con interpolación
        currentX = Mathf.SmoothDamp(currentX, targetX, ref currentVelocityX, rotationSmoothTime);
        currentY = Mathf.SmoothDamp(currentY, targetY, ref currentVelocityY, rotationSmoothTime);

        // Limitar ángulo vertical
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // Scroll del mouse (zoom in/out)
        float scroll = Input.mouseScrollDelta.y;
        distance -= scroll * 2f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calcular rotación y posición
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 position = target.position + rotation * direction + offset;

        transform.position = position;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}

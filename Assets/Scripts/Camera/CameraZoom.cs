using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cinemachineCam;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minFOV = 35f;
    public float maxFOV = 60f;

    private void Update()
    {
        if (cinemachineCam == null) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float currentFOV = cinemachineCam.Lens.FieldOfView;
        currentFOV -= scroll * zoomSpeed;
        currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);

        cinemachineCam.Lens.FieldOfView = currentFOV;
    }
}

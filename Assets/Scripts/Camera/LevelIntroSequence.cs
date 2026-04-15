using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class LevelIntroSequence : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineCamera gameplayCamera;
    public CinemachineCamera introCamera;

    [Header("Intro Points")]
    public Transform introStart;
    public Transform introEnd;

    [Header("Timing")]
    public float holdAtStart = 1.5f;
    public float moveDuration = 3f;
    public float holdAtEnd = 1f;

    [Header("Zoom")]
    public float startFOV = 35f;
    public float endFOV = 55f;

    private IEnumerator Start()
    {
        if (introCamera == null || gameplayCamera == null || introStart == null || introEnd == null)
            yield break;

        // Activar intro
        introCamera.Priority = 30;
        gameplayCamera.Priority = 10;

        // Posición inicial
        introCamera.transform.position = introStart.position;
        introCamera.transform.rotation = introStart.rotation;
        introCamera.Lens.FieldOfView = startFOV;

        yield return new WaitForSeconds(holdAtStart);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // suavizado
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            introCamera.transform.position = Vector3.Lerp(introStart.position, introEnd.position, smoothT);
            introCamera.transform.rotation = Quaternion.Slerp(introStart.rotation, introEnd.rotation, smoothT);
            introCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, smoothT);

            yield return null;
        }

        yield return new WaitForSeconds(holdAtEnd);

        // Volver a gameplay
        introCamera.Priority = 0;
        gameplayCamera.Priority = 10;
    }
}

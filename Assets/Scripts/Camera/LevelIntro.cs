using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class LevelIntro : MonoBehaviour
{
    public CinemachineCamera gameplayCamera;
    public CinemachineCamera introCamera;
    public float introDuration = 3f;

    private IEnumerator Start()
    {
        if (introCamera != null)
            introCamera.Priority = 30;

        if (gameplayCamera != null)
            gameplayCamera.Priority = 10;

        yield return new WaitForSeconds(introDuration);

        if (introCamera != null)
            introCamera.Priority = 0;

        if (gameplayCamera != null)
            gameplayCamera.Priority = 10;
    }
}

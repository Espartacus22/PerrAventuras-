using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class ZoneTrigger : MonoBehaviour
{
    public CinemachineCamera gameplayCamera;
    public CinemachineCamera zoneCamera;
    public float duration = 2.5f;
    public bool triggerOnlyOnce = true;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnlyOnce && alreadyTriggered) return;

        alreadyTriggered = true;
        StartCoroutine(PlayZoneCamera());
    }

    private IEnumerator PlayZoneCamera()
    {
        if (zoneCamera != null)
            zoneCamera.Priority = 20;

        if (gameplayCamera != null)
            gameplayCamera.Priority = 10;

        yield return new WaitForSeconds(duration);

        if (zoneCamera != null)
            zoneCamera.Priority = 0;

        if (gameplayCamera != null)
            gameplayCamera.Priority = 10;
    }
}

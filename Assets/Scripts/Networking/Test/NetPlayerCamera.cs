using Fusion;
using UnityEngine;

public class NetPlayerCamera : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListenerComp;

    public override void Spawned()
    {
        bool isLocalPlayer = HasInputAuthority;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(isLocalPlayer);

        if (audioListenerComp != null)
            audioListenerComp.enabled = isLocalPlayer;
    }
}

using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class NetworkSceneTransition : NetworkBehaviour
{
    [SerializeField] private int sceneIndexToLoad = 4;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireInput = true;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    private bool playerInside;

    private void Update()
    {
        if (!requireInput) return;

        if (playerInside && Input.GetKeyDown(interactionKey))
        {
            LoadNetworkScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        if (!requireInput)
        {
            LoadNetworkScene();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
    }

    private void LoadNetworkScene()
    {
        if (Runner == null) return;

        if (!Runner.IsServer)
        {
            Debug.Log("[SCENE] Solo el Host/Server puede cambiar la escena.");
            return;
        }

        Debug.Log("[SCENE] Cargando CityScene por Fusion...");
        Runner.LoadScene(SceneRef.FromIndex(sceneIndexToLoad));
    }
}

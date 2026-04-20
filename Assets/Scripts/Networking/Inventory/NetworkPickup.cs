using Fusion;
using UnityEngine;

public class NetworkPickup : NetworkBehaviour
{
    [SerializeField] private ItemData itemData;

    public int ItemId => itemData ? itemData.id : -1;

    public void ServerCollect()
    {
        if (Runner != null && Object)
        {
            Debug.Log($"[PICKUP] Despawneando {itemData.itemName}");
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Runner == null || Object == null)
        {
            Debug.LogWarning("[PICKUP] Runner u Object es null, se ignora trigger.");
            return;
        }

        if (!Runner.IsServer)
            return;

        Debug.Log($"[PICKUP] Trigger con {other.name}");

        Transform current = other.transform;

        while (current != null)
        {
            if (current.TryGetComponent<NetworkObject>(out var playerNetObj))
            {
                Debug.Log($"[PICKUP] Encontrado NetworkObject en {current.name}");

                var inventory = playerNetObj.GetComponent<InventorySystem>();

                if (inventory == null)
                {
                    Debug.LogWarning($"[PICKUP] {playerNetObj.name} no tiene InventorySystem.");
                    return;
                }

                Debug.Log($"[PICKUP] Procesando TryPickup en el servidor para {playerNetObj.name}");
                inventory.TryPickup(Object);
                return;
            }

            current = current.parent;
        }

        Debug.LogWarning($"[PICKUP] No se encontró NetworkObject válido en {other.name}");
    }
}
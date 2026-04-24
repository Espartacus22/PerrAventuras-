using Fusion;
using UnityEngine;

public class NetworkPickup : NetworkBehaviour
{
    [SerializeField] private ItemData itemData;

    public int ItemId => itemData ? itemData.id : -1;

    public void ServerCollect()
    {
        if (Runner != null && Object != null)
        {
            string pickupName = itemData != null ? itemData.itemName : gameObject.name;
            Debug.Log($"[PICKUP] Despawneando {pickupName}");
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Runner == null || Object == null) return;
        if (!Runner.IsServer) return;
        if (itemData == null) return;

        Debug.Log($"[PICKUP] Trigger con {other.name}");

        Transform current = other.transform;
        while (current != null)
        {
            if (current.TryGetComponent<NetworkObject>(out var playerNetObj))
            {
                Debug.Log($"[PICKUP] Encontrado NetworkObject en {current.name}");

                var inventory = playerNetObj.GetComponent<InventorySystem>();
                if (inventory != null)
                {
                    Debug.Log($"[PICKUP] Procesando TryPickup en el servidor para {playerNetObj.name}");
                    inventory.TryPickup(Object);
                }

                return;
            }

            current = current.parent;
        }
    }
}
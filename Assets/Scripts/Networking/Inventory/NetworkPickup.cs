using UnityEngine;
using Fusion;

public class NetworkPickup : NetworkBehaviour
{
    [Header("Configuración del Ítem")]
    [SerializeField] private ItemData itemData; // ScriptableObject de Item

    // Exponemos el ItemData por si otros componentes necesitan consultarlo
    public ItemData ItemData => itemData;

    // Propiedad pública que InventorySystem necesita para leer el ID
    public int ItemId => itemData != null ? itemData.id : -1;

    private void OnTriggerEnter(Collider other)
    {
        // Blindaje Multiplayer: Solo el Servidor procesa las colisiones físicas.
        if (Runner == null || Object == null) return;
        if (!Runner.IsServer) return;

        if (itemData == null)
        {
            Debug.LogWarning($"[NetworkPickup] El objeto {gameObject.name} no tiene un ItemData asignado.");
            return;
        }

        // Verificar si el objeto que entró al trigger es realmente un jugador
        if (other.CompareTag("Player"))
        {
            InventorySystem inventory = other.GetComponent<InventorySystem>();
            MissionController mission = other.GetComponent<MissionController>();

            if (inventory != null)
            {
                
                // Al delegar el trabajo a esa función, ya no tocamos la variable privada 'Items' desde acá.
                if (inventory.AddItem(itemData.id))
                {
                    Debug.Log($"[Servidor] Ítem '{itemData.itemName}' añadido al inventario del jugador.");

                    // EVENTO DE MISIÓN: Le avisamos al MissionController que sume el progreso de la recolección
                    if (mission != null)
                    {
                        string missionEventId = $"Pick_{itemData.itemName}";
                        mission.UpdateProgress(missionEventId, 1);
                    }

                    // Despawnear de la red: Eliminamos el objeto físico del mapa en todos los clientes
                    ServerCollect();
                }
                else
                {
                    Debug.Log($"[Servidor] El jugador intentó agarrar {itemData.itemName} pero tiene el inventario lleno.");
                }
            }
        }
    }

    /// <summary>
    /// Elimina de forma segura el objeto de red en el servidor.
    /// </summary>
    public void ServerCollect()
    {
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
    }
}
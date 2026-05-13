using UnityEngine;
using Fusion;
using Networking;

public class BreakableChest : NetworkBehaviour
{
    [Header("Configuracion de Vida")]
    public int maxHP = 30;

    // Usamos [Networked] para que todos los jugadores vean la misma vida
    [Networked] protected int networkedHP { get; set; }

    [Header("Configuracion de Loot Aleatorio")]
    [Tooltip("IDs de los items en tu ItemDatabase que pueden salir de este cofre")]
    public int[] possibleItemIDs;

    [Tooltip("Cantidad de objetos que soltara al romperse")]
    public int itemsToDrop = 1;

    public override void Spawned()
    {
        // El servidor (StateAuthority) es el unico que setea la vida inicial
        if (HasStateAuthority)
        {
            networkedHP = maxHP;
        }
    }

    public void TakeDamage(int amount)
    {
        // Solo el servidor procesa el daño para evitar trampas
        if (!HasStateAuthority) return;

        networkedHP -= amount;

        if (networkedHP <= 0)
        {
            EjecutarRotura();
        }
    }

    protected virtual void EjecutarRotura()
    {
        // Soltamos los items basados en los IDs de la lista
        for (int i = 0; i < itemsToDrop; i++)
        {
            SpawnearItemAlAzar();
        }

        // En Photon Fusion usamos Despawn para eliminar el objeto de la red
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
    }

    private void SpawnearItemAlAzar()
    {
        if (possibleItemIDs == null || possibleItemIDs.Length == 0) return;

        // Elegimos un ID aleatorio de la lista que configuraste en el Inspector
        int selectedID = possibleItemIDs[Random.Range(0, possibleItemIDs.Length)];

        // Buscamos la data en tu ItemDatabase
        var itemData = ItemDatabase.GetItem(selectedID);

        if (itemData != null && itemData.pickupPrefab != null)
        {
            NetworkObject prefabObj = itemData.pickupPrefab.GetComponent<NetworkObject>();

            if (prefabObj != null)
            {
                // Un pequeño offset para que no aparezcan todos en el mismo pixel
                Vector3 spawnPos = transform.position + (Random.insideUnitSphere * 0.5f);
                spawnPos.y = transform.position.y + 0.5f;

                Runner.Spawn(prefabObj, spawnPos, Quaternion.identity);
            }
        }
    }
}
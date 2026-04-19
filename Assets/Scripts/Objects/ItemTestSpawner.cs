using UnityEngine;
using Fusion;
using System.Collections; // Necesario para la corrutina

public class ItemTestSpawner : NetworkBehaviour
{
    [Header("Prefabs de Items de Inventario")]
    public NetworkObject[] itemPrefabs;

    [Header("Configuración de Spawneo")]
    public float spawnRadius = 5f;
    public int amountToSpawn = 3;

    public override void Spawned()
    {
        // Solo el que tiene la autoridad (Host/Master) spawnea
        if (HasStateAuthority)
        {
            Debug.Log("<color=cyan>ItemSpawner:</color> Tengo autoridad. Esperando al Runner...");
            // Usamos una corrutina para esperar a que el Runner esté 100% operativo
            StartCoroutine(WaitAndSpawn());
        }
    }

    IEnumerator WaitAndSpawn()
    {
        // Esperamos un par de frames para asegurar que la red esté estable
        yield return new WaitForSeconds(0.5f);

        if (Runner != null && Runner.IsRunning)
        {
            Debug.Log("<color=cyan>ItemSpawner:</color> ¡Runner listo! Iniciando spawneo.");
            SpawnInitialItems();
        }
        else
        {
            Debug.LogError("<color=red>ItemSpawner:</color> El Runner no está listo o no existe.");
        }
    }

    void SpawnInitialItems()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No hay prefabs asignados en el ItemSpawner.");
            return;
        }

        for (int i = 0; i < amountToSpawn; i++)
        {
            NetworkObject prefabToSpawn = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
            // Lo subimos a 2 metros de altura para verlo caer
            Vector3 spawnPos = transform.position + new Vector3(randomPoint.x, 2f, randomPoint.y);

            Runner.Spawn(prefabToSpawn, spawnPos, Quaternion.identity);
            Debug.Log($"<color=green>ItemSpawner:</color> Spawneado {prefabToSpawn.name} en {spawnPos}");
        }
    }
}


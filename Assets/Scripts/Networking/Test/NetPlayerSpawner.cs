using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetPlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private readonly Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new();

    public void SpawnPlayer(PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (spawnedPlayers.ContainsKey(player))
            return;

        Vector3 spawnPosition = GetSpawnPosition(player);
        NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

        spawnedPlayers.Add(player, playerObject);

        Debug.Log($"Player spawned: {player.PlayerId}");
    }

    public void DespawnPlayer(PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (spawnedPlayers.TryGetValue(player, out NetworkObject playerObject))
        {
            runner.Despawn(playerObject);
            spawnedPlayers.Remove(player);
            Debug.Log($"Player despawned: {player.PlayerId}");
        }
    }

    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Vector3.zero;

        int index = spawnedPlayers.Count % spawnPoints.Length;
        return spawnPoints[index].position;
    }
}

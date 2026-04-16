using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDelay = 2f;

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

        NetPlayerHealth health = playerObject.GetComponent<NetPlayerHealth>();
        if (health != null)
        {
            health.SetSpawner(this, player);
        }

        Debug.Log($"Player spawned: {player.PlayerId} at {spawnPosition}");
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

    public void RequestRespawn(PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        StartCoroutine(RespawnRoutine(player));
    }

    private IEnumerator RespawnRoutine(PlayerRef player)
    {
        DespawnPlayer(player);
        yield return new WaitForSeconds(respawnDelay);
        SpawnPlayer(player);
    }

    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Vector3.zero;

        int index = player.PlayerId % spawnPoints.Length;
        return spawnPoints[index].position;
    }
}

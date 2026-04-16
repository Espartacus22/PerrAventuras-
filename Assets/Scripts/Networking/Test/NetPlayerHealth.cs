using Fusion;
using UnityEngine;

public class NetPlayerHealth : NetworkBehaviour
{
    [Networked] public int CurrentHealth { get; private set; } = 100;
    [Networked] private NetworkBool IsDead { get; set; }

    private NetPlayerSpawner spawner;
    private PlayerRef ownerPlayer;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentHealth = 100;
            IsDead = false;
        }
    }

    public void SetSpawner(NetPlayerSpawner newSpawner, PlayerRef playerRef)
    {
        spawner = newSpawner;
        ownerPlayer = playerRef;
    }

    public void TakeDamage(int damage)
    {
        if (!HasStateAuthority || IsDead)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        Debug.Log($"{Object.name} took {damage} damage. HP: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            Debug.Log($"{Object.name} died");

            if (spawner != null)
            {
                spawner.RequestRespawn(ownerPlayer);
            }
        }
    }

}

using Fusion;
using UnityEngine;

public class NetPlayerDamageAdapter : NetworkBehaviour, IDamageable
{
    public void TakeDamage(int amount)
    {
        if (!HasStateAuthority) return;

        PlayerLevel playerLevel = GetComponent<PlayerLevel>();
        if (playerLevel != null)
            playerLevel.TakeDamage(amount);
    }
}

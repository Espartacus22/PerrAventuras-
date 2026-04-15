using Fusion;
using UnityEngine;

public class NetPlayerHealth : NetworkBehaviour
{
    [Networked] public int CurrentHealth { get; private set; } = 100;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentHealth = 100;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!HasStateAuthority)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        Debug.Log($"{Object.name} took {damage} damage. HP: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Debug.Log($"{Object.name} died");
        }
    }
}

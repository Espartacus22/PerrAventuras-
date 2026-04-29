using UnityEngine;

public class PlayerLvlDamageAdapt : MonoBehaviour, IDamageable
{
    private PlayerLevel playerLevel;

    private void Awake()
    {
        playerLevel = GetComponent<PlayerLevel>();
    }

    public void TakeDamage(int amount)
    {
        if (playerLevel == null) return;
        playerLevel.TakeDamage(amount);
    }
}

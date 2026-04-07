using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Container")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Rewards")]
    public int hpReward = 0;
    public int shieldReward = 0;
    public int xpReward = 0;
    public int coinReward = 5;

    [Header("Optional")]
    public bool fullRestore = false;
    public GameObject breakEffect;
    public GameObject dropPrefab;

    private bool broken = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (broken) return;
        if (damage <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        if (broken) return;
        broken = true;

        PlayerLevel playerLevel = FindFirstObjectByType<PlayerLevel>();

        if (playerLevel != null)
        {
            if (fullRestore)
            {
                playerLevel.RestoreFullState();
            }
            else
            {
                if (hpReward > 0) playerLevel.Heal(hpReward);
                if (shieldReward > 0) playerLevel.RestoreShield(shieldReward);
            }

            if (xpReward > 0) playerLevel.AddXP(xpReward);
            if (coinReward > 0) playerLevel.AddCoins(coinReward);
        }

        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        if (dropPrefab != null)
            Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}

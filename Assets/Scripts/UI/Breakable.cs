using UnityEngine;

public class Breakable : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Rewards")]
    public int coinReward = 5;
    public int hpReward = 0;
    public int shieldReward = 0;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{name} recibió {damage} de daño.");
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Break();
        }
    }

    void Break()
    {
        PlayerLevel player = FindFirstObjectByType<PlayerLevel>();

        if (player != null)
        {
            player.AddCoins(coinReward);
            player.Heal(hpReward);
            player.RestoreShield(shieldReward);
        }

        Destroy(gameObject);
    }
}

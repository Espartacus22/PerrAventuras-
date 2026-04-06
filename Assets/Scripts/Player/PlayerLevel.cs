using UnityEngine;
using System.Collections;

public class PlayerLevel : MonoBehaviour
{
    [Header("Compatibilidad / referencias")]
    public CharacterType characterData;

    [Header("Nivel y experiencia")]
    public int currentLevel = 0;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Vida y escudo")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    public int maxShield = 0;
    public int currentShield = 0;

    [Header("Defensa")]
    public int defense = 0;

    [Header("Escalado por nivel")]
    public int healthPerLevel = 20;
    public int shieldPerLevel = 10;
    public int defensePerLevel = 1;

    [Header("Monedas")]
    public int currentCoins = 0;

    [Header("Debug")]
    public bool restoreFullOnLevelUp = true;

    private void Awake()
    {
        if (characterData == null)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
                characterData = movement.characterData;
        }
    }

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentShield = Mathf.Clamp(currentShield, 0, maxShield);

        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }

    // XP / NIVEL

    public void GainXP(int amount)
    {
        AddXP(amount);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        Debug.Log($"{name} ganó {amount} XP. XP actual: {currentXP}");

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        Debug.Log($"{name} subió a nivel {currentLevel}");

        ApplyLevelRewards();
    }

    private void ApplyLevelRewards()
    {
        maxHealth += healthPerLevel;
        maxShield += shieldPerLevel;
        defense += defensePerLevel;

        if (restoreFullOnLevelUp)
        {
            currentHealth = maxHealth;
            currentShield = maxShield;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentShield = Mathf.Min(currentShield, maxShield);
        }

        Debug.Log($"{name} mejoró stats -> HP: {maxHealth}, Escudo: {maxShield}, Defensa: {defense}");
    }

    // Coins

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        currentCoins += amount;
        Debug.Log($"{name} ganó {amount} monedas. Total: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (currentCoins < amount) return false;

        currentCoins -= amount;
        Debug.Log($"{name} gastó {amount} monedas. Total: {currentCoins}");
        return true;
    }

    // VIDA / DAÑO

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = Mathf.Max(damage - defense, 1);

        if (currentShield > 0)
        {
            int shieldDamage = Mathf.Min(currentShield, finalDamage);
            currentShield -= shieldDamage;
            finalDamage -= shieldDamage;
        }

        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
        }

        Debug.Log($"{name} recibió daño. HP: {currentHealth}, Escudo: {currentShield}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{name} se curó. HP actual: {currentHealth}");
    }

    public void RestoreShield(int amount)
    {
        if (amount <= 0) return;

        currentShield = Mathf.Min(currentShield + amount, maxShield);
        Debug.Log($"{name} recuperó escudo. Escudo actual: {currentShield}");
    }

    public void RestoreFullState()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
        Debug.Log($"{name} restauró vida y escudo al máximo.");
    }

    public int GetMaxHP()
    {
        return maxHealth;
    }

    public int GetCurrentHP()
    {
        return currentHealth;
    }

    public int GetCurrentShield()
    {
        return currentShield;
    }

    // MUERTE

    private void Die()
    {
        Debug.Log($"{name} murió.");

        PlayerRespawn respawn = GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.Respawn();
        }
        else
        {
            RestoreFullState();
            Debug.LogWarning("No se encontró PlayerRespawn en el Player.");
        }
    }
}

using UnityEngine;
using System.Collections;

public class PlayerLevel : MonoBehaviour
{
    [Header("Compatibilidad / referencias")]
    public CharacterType characterData;
    public PlayerRespawn respawn;

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

    [Header("Debug")]
    public bool restoreFullOnLevelUp = true;

    private void Awake()
    {
        // Si no está asignado en inspector, lo intentamos sacar del mismo objeto
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

    // -------------------------
    // XP / NIVEL
    // -------------------------

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

    // -------------------------
    // VIDA / DAÑO
    // -------------------------

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int finalDamage = Mathf.Max(damage - defense, 1);

        // Primero escudo
        if (currentShield > 0)
        {
            int shieldDamage = Mathf.Min(currentShield, finalDamage);
            currentShield -= shieldDamage;
            finalDamage -= shieldDamage;
        }

        // Después vida
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

    // Compatibilidad con scripts viejos
    public int GetMaxHP()
    {
        RestoreFullState();
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

    // -------------------------
    // MUERTE / RESPAWN
    // -------------------------

    private void Die()
    {
        Debug.Log($"{name} murió.");

        if (respawn != null)
        {
            respawn.OnPlayerDeath();
        }
        else
        {
            Debug.LogWarning($"{name}: no tiene PlayerRespawn asignado.");
        }
    }
}

using UnityEngine;
using Fusion;

public class PlayerLevel : NetworkBehaviour
{
    [Header("Compatibilidad / referencias")]
    public CharacterType characterData;

    [Header("Efectos Visuales (Cliente)")]
    public DamageFlash damageFlash;
    private int _lastVisibleHealth;

    // --- VARIABLES DE RED (Sincronizadas por el servidor) ---
    [Header("Nivel y experiencia")]
    [Networked] public int currentLevel { get; set; }
    [Networked] public int currentXP { get; set; }
    [Networked] public int xpToNextLevel { get; set; }

    [Header("Vida y escudo")]
    [Networked] public int maxHealth { get; set; }
    [Networked] public int currentHealth { get; set; }
    [Networked] public int maxShield { get; set; }
    [Networked] public int currentShield { get; set; }

    [Header("Defensa")]
    [Networked] public int defense { get; set; }

    [Header("Monedas")]
    [Networked] public int currentCoins { get; set; }

    // --- VARIABLES LOCALES FIJAS ---
    [Header("Escalado por nivel")]
    public int healthPerLevel = 20;
    public int shieldPerLevel = 10;
    public int defensePerLevel = 1;

    [Header("Debug")]
    public bool restoreFullOnLevelUp = true;

    private bool _isSpawned;

    private void Awake()
    {
        if (characterData == null)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
                characterData = movement.characterData;
        }
    }

    public override void Spawned()
    {
        _isSpawned = true;

        if (HasStateAuthority)
        {
            xpToNextLevel = 100;
            maxHealth = 100; // O characterData.baseHealth si lo prefieres
            currentHealth = maxHealth;
            maxShield = 0;
            currentShield = 0;
            defense = 0;
            currentCoins = 0;
        }

        _lastVisibleHealth = currentHealth;

        // ¡EL ARREGLO DE LA UI ESTÁ AQUÍ!
        if (HasInputAuthority)
        {
            // Le agregamos "UnityEngine." para que Fusion no tire el error CS0176
            PlayerHUD hud = UnityEngine.Object.FindFirstObjectByType<PlayerHUD>();

            if (hud != null)
            {
                hud.playerLevel = this;
                hud.RefreshHUD();
                Debug.Log("[UI] ¡Barras de vida y escudo conectadas!");
            }
        }
    }

    public override void Render()
    {
        if (_lastVisibleHealth != currentHealth)
        {
            if (currentHealth < _lastVisibleHealth)
            {
                if (damageFlash != null)
                {
                    damageFlash.Flash();
                }

                // Actualizamos la UI al recibir daño
                if (HasInputAuthority)
                {
                    PlayerHUD hud = UnityEngine.Object.FindFirstObjectByType<PlayerHUD>();
                    if (hud != null) hud.RefreshHUD();
                }
            }
            _lastVisibleHealth = currentHealth;
        }
    }

    public void GainXP(int amount) => AddXP(amount);

    public void AddXP(int amount)
    {
        if (!HasStateAuthority) return;
        if (amount <= 0) return;
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
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
    }

    public void AddCoins(int amount)
    {
        if (!HasStateAuthority) return;
        if (amount <= 0) return;
        currentCoins += amount;
    }

    public bool SpendCoins(int amount)
    {
        if (!HasStateAuthority) return false;
        if (amount <= 0) return false;
        if (currentCoins < amount) return false;

        currentCoins -= amount;
        return true;
    }

    public void TakeDamage(int damage)
    {
        if (!HasStateAuthority) return;
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (!HasStateAuthority) return;
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void RestoreShield(int amount)
    {
        if (!HasStateAuthority) return;
        if (amount <= 0) return;
        currentShield = Mathf.Min(currentShield + amount, maxShield);
    }

    public void RestoreFullState()
    {
        if (!HasStateAuthority) return;
        currentHealth = maxHealth;
        currentShield = maxShield;
    }

    public int GetMaxHP() => (!_isSpawned || Object == null) ? 0 : maxHealth;
    public int GetCurrentHP() => (!_isSpawned || Object == null) ? 0 : currentHealth;
    public int GetCurrentShield() => (!_isSpawned || Object == null) ? 0 : currentShield;

    private void Die()
    {
        PlayerRespawn respawn = GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.Respawn();
        }
        else
        {
            RestoreFullState();
        }
    }
}



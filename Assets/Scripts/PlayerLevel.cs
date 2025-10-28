using UnityEngine;
using System;

/// <summary>
/// Sistema de nivel y experiencia del jugador.
/// Escala vida, escudo, daño y desbloquea habilidades según el nivel.
/// Compatible con CharacterType y PlayerHealthLocal.
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    [Header("Referencias")]
    public CharacterType characterData;           // Datos base del personaje
    public PlayerHealthLocal playerHealth;        // Para actualizar HP y escudo
    public PlayerLocal playerController;          // Para desbloquear habilidades

    [Header("Nivel y Experiencia")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int[] xpRequiredPerLevel = { 0, 100, 250, 500, 1000, 2000 };

    [Header("Multiplicadores")]
    [Tooltip("Cuánto aumenta la vida máxima por nivel")]
    public float hpPerLevel = 10f;
    [Tooltip("Cuánto aumenta el daño base por nivel")]
    public float damageMultiplier = 0.15f;
    [Tooltip("Cuánto aumenta la defensa por nivel")]
    public float defensePerLevel = 2f;

    [Header("Eventos")]
    public Action<int> OnLevelUp; // Evento que notifica al subir de nivel

    private void Start()
    {
        if (characterData == null)
            Debug.LogWarning("CharacterData no asignado en PlayerLevel");

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealthLocal>();

        if (playerController == null)
            playerController = GetComponent<PlayerLocal>();

        ApplyLevelStats();
    }


    // Añade experiencia al jugador y controla la subida de nivel.
    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Ganaste {amount} XP. Total actual: {currentXP}");

        // Subir de nivel si supera el XP necesario
        while (currentLevel < xpRequiredPerLevel.Length - 1 &&
               currentXP >= xpRequiredPerLevel[currentLevel + 1])
        {
            currentLevel++;
            Debug.Log($"Subiste a nivel {currentLevel}!");
            OnLevelUp?.Invoke(currentLevel);
            ApplyLevelStats();
            UnlockAbilities();
        }
    }


    // Aplica mejoras de estadísticas basadas en el nivel actual.
    public void ApplyLevelStats()
    {
        if (characterData == null || playerHealth == null) return;

        // Vida escalada
        playerHealth.maxHealth = Mathf.RoundToInt(characterData.maxHP + (currentLevel - 1) * hpPerLevel);
        playerHealth.currentHealth = playerHealth.maxHealth;

        // Escudo escalado (si aplica)
        playerHealth.maxShield = Mathf.RoundToInt(characterData.maxShield + (currentLevel - 1) * defensePerLevel);
        playerHealth.currentShield = playerHealth.maxShield;

        playerHealth.UpdateUI();
    }


    // Retorna el daño escalado en base al nivel actual.
    public float GetScaledDamage(float baseDamage)
    {
        return baseDamage + (baseDamage * (currentLevel * damageMultiplier));
    }


    // Determina si una habilidad está disponible según el nivel.
    public bool IsAbilityUnlocked(int requiredLevel)
    {
        return currentLevel >= requiredLevel;
    }

    /// <summary>
    /// Desbloquea habilidades progresivamente.
    /// </summary>
    private void UnlockAbilities()
    {
        if (playerController == null || characterData == null) return;

        // Ejemplo: desbloqueo progresivo
        if (currentLevel >= 2 && characterData.canDash)
        {
            playerController.EnableDash(true);
            Debug.Log("Dash desbloqueado!");
        }

        if (currentLevel >= 3 && characterData.canDoubleJump)
        {
            playerController.EnableDoubleJump(true);
            Debug.Log("Doble salto desbloqueado!");
        }

        if (currentLevel >= 4 && characterData.canBlock)
        {
            playerController.EnableBlock(true);
            Debug.Log("Bloqueo desbloqueado!");
        }
    }

    /// <summary>
    /// Aumenta el daño recibido según el nivel (enemigos).
    /// </summary>
    public float GetDamageReductionFactor()
    {
        // Reduce daño hasta un 30% máximo
        return 1f - Mathf.Min(0.3f, currentLevel * 0.02f);
    }

#if UNITY_EDITOR
    [ContextMenu("Agregar 100 XP")]
    private void DebugAddXP()
    {
        GainXP(100);
    }

    [ContextMenu("Subir un nivel manualmente")]
    private void DebugLevelUp()
    {
        currentXP = xpRequiredPerLevel[Mathf.Min(currentLevel + 1, xpRequiredPerLevel.Length - 1)];
        GainXP(0);
    }
#endif
}
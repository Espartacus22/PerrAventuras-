using UnityEngine;
using System.Collections;

public class PlayerLevel : MonoBehaviour
{
    public CharacterType characterData;
    public int currentLevel = 1;
    public int currentXP = 0;
    public int[] xpRequiredPerLevel = { 0, 100, 250, 500, 800, 1200 };

    //Player Life
    [Header("Life")]
    public int currentHP;
    public bool hasShield = false;

    [Header("Respawn")]
    public PlayerRespawn respawn;

    // PARPADEO ROJO AL RECIBIR DAÑO
    [Header("Efecto Daño")]
    private Renderer playerRenderer;
    private Color originalColor;
    private Coroutine blinkCoroutine;

    public int GetMaxHP() => characterData.hp + currentLevel * 10;
    public int GetDefense() => (hasShield ? 15 : 0) + currentLevel * 2;
    public float GetFinalDamage(float baseDamage) => baseDamage + currentLevel * 0.1f;

    private void Start()
    {
        //Start with maximum life
        currentHP = GetMaxHP();

        // Guardar color original para parpadeo
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GainXP(100); // Ganás 100 XP al presionar X - You gain 100 XP by pressing X
        }
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"XP actual: {currentXP}, Nivel actual: {currentLevel}");
        while (currentLevel < xpRequiredPerLevel.Length - 1 &&
               currentXP >= xpRequiredPerLevel[currentLevel + 1])
        {
            currentLevel++;
            Debug.Log($"Subiste a nivel {currentLevel}");
            // Optional: When leveling up, heal a little
            currentHP = Mathf.Min(GetMaxHP(), currentHP + 10);
        }
    }

    [ContextMenu("Test XP Gain")]
    public void TestGainXP()
    {
        GainXP(0); // Esto fuerza la evaluación sin sumar XP
    }

    [ContextMenu("Forzar evaluación de leveo")]
    public void ForceLevelCheck()
    {
        while (currentLevel < xpRequiredPerLevel.Length - 1 &&
               currentXP >= xpRequiredPerLevel[currentLevel + 1])
        {
            currentLevel++;
            Debug.Log($"Subiste a nivel {currentLevel}");
        }
    }

    [ContextMenu("Resetear XP y nivel")]
    public void ResetLevel()
    {
        currentLevel = 1;
        currentXP = 0;
        Debug.Log("Nivel y XP reseteados");
    }

    public bool IsAttackUnlocked(int requiredLevel) => currentLevel >= requiredLevel;

    public void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(0, amount - GetDefense());
        currentHP -= finalDamage;
        Debug.Log($"Player recibió {finalDamage} de daño. HP actual: {currentHP}");

        // PARPADEO ROJO AL RECIBIR DAÑO
        if (playerRenderer != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRed());
        }

        if (currentHP <= 0)
        {
            currentHP = 0;  // ASEGURAR HP = 0 (no negativo)
            Debug.Log("Player dead!!!");

            if (respawn != null)
            {
                respawn.OnPlayerDeath();
                currentHP = GetMaxHP();  // VIDA RESTAURADA AL 100%
                Debug.Log($"Player respawneado! HP restaurado: {currentHP}");
            }
            else
            {
                Debug.LogWarning("PlayerRespawn no asignado en PlayerLevel");
            }
        }
    }

    private IEnumerator BlinkRed()
    {
        for (int i = 0; i < 4; i++)
        {
            playerRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            playerRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }
        blinkCoroutine = null;
    }
}

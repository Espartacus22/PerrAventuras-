using UnityEngine;
using System.Collections;

public class PlayerLevel : MonoBehaviour
{
    public CharacterType characterData;
    public int currentLevel = 0;
    public int currentXP = 0;
    public int[] xpRequiredPerLevel = { 0, 100, 250, 500, 800, 1200 };

    // Player Life
    [Header("Life")]
    public int currentHP;
    public bool hasShield = false;

    // Vida extra por niveles (ej: +100 en nivel 2)
    [SerializeField] private int extraMaxHP = 0;

    [Header("Respawn")]
    public PlayerRespawn respawn;

    // PARPADEO ROJO AL RECIBIR DANO
    [Header("Efecto Daño")]
    private Renderer playerRenderer;
    private Color originalColor;
    private Coroutine blinkCoroutine;

    // ---------- STATS BASE ----------

    // HP maximo = HP base del CharacterType + bonus por nivel
    public int GetMaxHP() => characterData.hp + extraMaxHP;

    public int GetDefense() => (hasShield ? 15 : 0) + currentLevel * 2;
    public float GetFinalDamage(float baseDamage) => baseDamage + currentLevel * 0.1f;

    private void Start()
    {
        // Comenzar siempre con la vida al maximo actual
        currentHP = GetMaxHP();

        // Guardar color original para parpadeo
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
    }

    void Update()
    {
        // Debug XP (tecla X)
        if (Input.GetKeyDown(KeyCode.X))
        {
            GainXP(100); // Ganas 100 XP al presionar X
        }
    }

    // ---------- XP / NIVELES ----------

    public void GainXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"XP actual: {currentXP}, Nivel actual: {currentLevel}");

        while (currentLevel < xpRequiredPerLevel.Length - 1 &&
               currentXP >= xpRequiredPerLevel[currentLevel + 1])
        {
            currentLevel++;
            Debug.Log($"Subiste a nivel {currentLevel}");

            OnLevelUp(currentLevel);

            // Curacion leve al subir nivel (ademas de posibles efectos en OnLevelUp)
            currentHP = Mathf.Min(GetMaxHP(), currentHP + 10);
        }
    }

    private void OnLevelUp(int newLevel)
    {
        // Nivel 1 → desbloquea doble salto
        if (newLevel == 1)
        {
            var pm = GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.UnlockDoubleJump();
                Debug.Log("DOBLE SALTO desbloqueado por NIVEL 1");
            }
        }

        // Nivel 2 → +100 HP máximo
        if (newLevel == 2)
        {
            extraMaxHP += 100;              // ahora el máximo sube (ej: de 100 a 200)
            currentHP = GetMaxHP();         // rellenamos vida al nuevo máximo
            Debug.Log($"Nivel 2 alcanzado → HP máximo ahora: {currentHP}");
        }

        // Futuro: if (newLevel == 3) { ... }
    }

    [ContextMenu("Test XP Gain")]
    public void TestGainXP()
    {
        GainXP(0); // fuerza evaluación sin sumar XP
    }

    [ContextMenu("Forzar evaluación de leveo")]
    public void ForceLevelCheck()
    {
        while (currentLevel < xpRequiredPerLevel.Length - 1 &&
               currentXP >= xpRequiredPerLevel[currentLevel + 1])
        {
            currentLevel++;
            Debug.Log($"Subiste a nivel {currentLevel}");
            OnLevelUp(currentLevel);
        }
    }

    [ContextMenu("Resetear XP y nivel")]
    public void ResetLevel()
    {
        currentLevel = 0;
        currentXP = 0;
        extraMaxHP = 0;
        currentHP = GetMaxHP();
        Debug.Log("Nivel, XP y HP reseteados");
    }

    public bool IsAttackUnlocked(int requiredLevel) => currentLevel >= requiredLevel;

    // ---------- DAÑO / CURA ----------

    public void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(0, amount - GetDefense());
        currentHP -= finalDamage;
        Debug.Log($"Player recibio {finalDamage} de dano. HP actual: {currentHP}");

        // PARPADEO ROJO AL RECIBIR DANO
        if (playerRenderer != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRed());
        }

        if (currentHP <= 0)
        {
            currentHP = 0;  // asegurar HP = 0 (no negativo)
            Debug.Log("Player dead!!!");

            if (respawn != null)
            {
                respawn.OnPlayerDeath();
                currentHP = GetMaxHP();  // vida restaurada al 100%
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

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(GetMaxHP(), currentHP + amount);
        Debug.Log($"Player curado por {amount}. HP actual: {currentHP}");
    }
}

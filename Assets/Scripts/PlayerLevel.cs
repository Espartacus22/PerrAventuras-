using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public CharacterType characterData;
    public int currentLevel = 1;
    public int currentXP = 0;
    public int[] xpRequiredPerLevel = { 0, 100, 250, 500, 800, 1200 };

    public bool hasShield = false;

    public float GetMaxHP() => characterData.hp + currentLevel * 10;
    public int GetDefense() => (hasShield ? 15 : 0) + currentLevel * 2;
    public float GetFinalDamage(float baseDamage) => baseDamage + currentLevel * 0.1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GainXP(100); // Ganás 100 XP al presionar X
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
}

using UnityEngine;

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

    public int GetMaxHP() => characterData.hp + currentLevel * 10;
    public int GetDefense() => (hasShield ? 15 : 0) + currentLevel * 2;
    public float GetFinalDamage(float baseDamage) => baseDamage + currentLevel * 0.1f;

    private void Start()
    {
        //Start with maximum life
        currentHP = GetMaxHP();
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

   public void TakeDamage(int damage)
    {
        int defense = GetDefense();
        int finalDamage = Mathf.Max(1, damage - defense);

        currentHP =Mathf.Max(0, currentHP - finalDamage);
        Debug.Log($"Player Recibe {finalDamage} de daño. HP Actual: {currentHP}");

        if (currentHP == 0)
        {
            Debug.Log("Player dead!!!");
        }
    }
}

using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public CharacterType characterData;
    public int currentLevel = 1;
    public int currentXP = 0;
    public int[] xpRequiredPerLevel = { 0, 100, 250, 500, 800, 1200 };

    public bool hasShield = false;

    public int GetMaxHP() => characterData.hp + currentLevel * 10;
    public int GetDefense() => (hasShield ? 15 : 0) + currentLevel * 2;
    public float GetFinalDamage(float baseDamage) => baseDamage + currentLevel * 0.1f;

    public void GainXP(int amount)
    {
        currentXP += amount;
        while (currentLevel < xpRequiredPerLevel.Length - 1 &&
               currentXP >= xpRequiredPerLevel[currentLevel + 1])
        {
            currentLevel++;
            Debug.Log($"Subiste a nivel {currentLevel}");
        }
    }

    public bool IsAttackUnlocked(int requiredLevel) => currentLevel >= requiredLevel;
}

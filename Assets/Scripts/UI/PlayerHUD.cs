using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Bars")]
    public Slider hpBar;
    public Slider shieldBar;
    public Slider xpBar;

    [Header("Texts")]
    public TMP_Text hpText;
    public TMP_Text shieldText;
    public TMP_Text xpText;
    public TMP_Text coinsText;

    [Header("References")]
    public PlayerLevel playerLevel;

    private void Update()
    {
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (playerLevel == null) return;

        int maxHP = playerLevel.GetMaxHP();
        int currentHP = playerLevel.GetCurrentHP();
        int currentShield = playerLevel.GetCurrentShield();

        if (maxHP <= 0) return;

        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;
        hpText.text = "HP: " + currentHP + " / " + maxHP;

        shieldBar.maxValue = playerLevel.maxShield;
        shieldBar.value = currentShield;
        shieldText.text = "Shield: " + currentShield + " / " + playerLevel.maxShield;

        xpBar.maxValue = playerLevel.xpToNextLevel;
        xpBar.value = playerLevel.currentXP;
        xpText.text = "XP: " + playerLevel.currentXP + " / " + playerLevel.xpToNextLevel;

        coinsText.text = "Coins: " + playerLevel.currentCoins;
    }
}

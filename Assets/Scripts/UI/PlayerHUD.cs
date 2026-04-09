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

    private void Start()
    {
        RefreshHUD();
    }

    private void Update()
    {
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        if (playerLevel == null) return;

        // HP
        hpBar.maxValue = playerLevel.maxHealth;
        hpBar.value = playerLevel.currentHealth;
        hpText.text = "HP: " + playerLevel.currentHealth + " / " + playerLevel.maxHealth;

        // Shield
        shieldBar.maxValue = playerLevel.maxShield;
        shieldBar.value = playerLevel.currentShield;
        shieldText.text = "Shield: " + playerLevel.currentShield + " / " + playerLevel.maxShield;

        // XP
        xpBar.maxValue = playerLevel.xpToNextLevel;
        xpBar.value = playerLevel.currentXP;
        xpText.text = "XP: " + playerLevel.currentXP + " / " + playerLevel.xpToNextLevel;

        // Coins
        coinsText.text = "Coins: " + playerLevel.currentCoins;
    }
}

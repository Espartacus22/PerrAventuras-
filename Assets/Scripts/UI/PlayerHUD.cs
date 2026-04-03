using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD
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

    [Header("Test Values")]
    public float currentHP = 100f;
    public float maxHP = 100f;

    public float currentShield = 50f;
    public float maxShield = 50f;

    public int currentXP = 0;
    public int xpToNextLevel = 100;

    public int currentCoins = 0;

    private void Start()
    {
        RefreshHUD();
    }

    private void Update()
    {
        // Solo para test rápido en Play Mode
        TestInput();

        RefreshHUD();
    }

    public void RefreshHUD()
    {
        // HP
        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;
        hpText.text = "HP: " + Mathf.RoundToInt(currentHP) + " / " + Mathf.RoundToInt(maxHP);

        // Shield
        shieldBar.maxValue = maxShield;
        shieldBar.value = currentShield;
        shieldText.text = "Shield: " + Mathf.RoundToInt(currentShield) + " / " + Mathf.RoundToInt(maxShield);

        // XP
        xpBar.maxValue = xpToNextLevel;
        xpBar.value = currentXP;
        xpText.text = "XP: " + currentXP + " / " + xpToNextLevel;

        // Coins
        coinsText.text = "Coins: " + currentCoins;
    }

    private void TestInput()
    {
        // Bajar HP
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentHP -= 10f;
            if (currentHP < 0) currentHP = 0;
        }

        // Subir HP
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentHP += 10f;
            if (currentHP > maxHP) currentHP = maxHP;
        }

        // Bajar escudo
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentShield -= 10f;
            if (currentShield < 0) currentShield = 0;
        }

        // Subir escudo
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentShield += 10f;
            if (currentShield > maxShield) currentShield = maxShield;
        }

        // Sumar XP
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currentXP += 10;
            if (currentXP > xpToNextLevel) currentXP = xpToNextLevel;
        }

        // Restar XP
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            currentXP -= 10;
            if (currentXP < 0) currentXP = 0;
        }

        // Sumar monedas
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            currentCoins += 5;
        }

        // Restar monedas
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            currentCoins -= 5;
            if (currentCoins < 0) currentCoins = 0;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthLocal : MonoBehaviour
{
    [Header("Referencias")]
    public CharacterType characterData;
    public Image healthBar;
    public Image shieldBar;

    [Header("HP & Shield")]
    public int maxHealth;
    public int currentHealth;
    public int maxShield;
    public int currentShield;

    [Range(0f, 1f)]
    public float shieldDamageAbsorb = 1f;
    private float shieldRegenDelay;
    private float shieldRegenRate;

    private bool isInvulnerable = false;
    private float lastDamageTime = -100f;
    private Coroutine regenCoroutine;

    void Start()
    {
        InitializeFromData();
    }

    void InitializeFromData()
    {
        if (characterData != null)
        {
            maxHealth = characterData.maxHP;
            maxShield = characterData.maxShield;
            shieldRegenDelay = characterData.shieldRegenDelay;
            shieldRegenRate = characterData.shieldRegenRate;
        }

        currentHealth = maxHealth;
        currentShield = maxShield;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        if (currentShield > 0)
        {
            float effectiveAbsorb = amount * shieldDamageAbsorb;
            int shieldAbsorbInt = Mathf.Min(currentShield, Mathf.CeilToInt(effectiveAbsorb));
            currentShield -= shieldAbsorbInt;

            int remainder = amount - Mathf.RoundToInt(shieldAbsorbInt / Mathf.Max(0.0001f, shieldDamageAbsorb));
            if (remainder > 0)
                currentHealth -= remainder;
        }
        else
        {
            currentHealth -= amount;
        }

        lastDamageTime = Time.time;
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);

        regenCoroutine = StartCoroutine(ShieldRegenDelayCoroutine());

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void ApplyDirectDamage(int amount)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;
        lastDamageTime = Time.time;

        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);

        regenCoroutine = StartCoroutine(ShieldRegenDelayCoroutine());
        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator ShieldRegenDelayCoroutine()
    {
        yield return new WaitForSeconds(shieldRegenDelay);
        while (currentShield < maxShield)
        {
            currentShield = Mathf.Min(maxShield, currentShield + Mathf.CeilToInt(shieldRegenRate * Time.deltaTime));
            UpdateUI();
            yield return null;
        }
    }

    public void AddShield(int amount)
    {
        currentShield = Mathf.Min(maxShield, currentShield + amount);
        UpdateUI();
    }

    IEnumerator InvulnerabilityFlash(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (shieldBar != null)
            shieldBar.fillAmount = (float)currentShield / maxShield;
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} murió.");
        // Acá podés añadir animación, respawn o cambio de cámara.
        Destroy(gameObject);
    }
}
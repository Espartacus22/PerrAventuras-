using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthLocal : MonoBehaviour
{
    [Header("HP")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Shield / Energy")]
    public int maxShield = 50;            // capacidad del escudo
    public int currentShield = 50;
    [Range(0f, 1f)]
    public float shieldDamageAbsorb = 1f; // % del daño que el shield absorbe (1 = absorbe totalmente hasta su valor)
    public float shieldRegenDelay = 3f;   // segundos sin recibir daño antes de empezar a regenerar
    public float shieldRegenRate = 8f;    // shield por segundo

    private bool isInvulnerable = false;
    private float lastDamageTime = -100f;
    private Coroutine regenCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        // Si hay shield, lo usamos primero (considerando shieldDamageAbsorb)
        if (currentShield > 0)
        {
            // Cuánto del daño puede absorber el shield
            float effectiveShieldAbsorb = amount * shieldDamageAbsorb;
            int shieldAbsorbInt = Mathf.Min(currentShield, Mathf.CeilToInt(effectiveShieldAbsorb));

            currentShield -= shieldAbsorbInt;

            // Si el shield no cubrió todo el daño, el remanente afecta la vida
            int remainder = amount - Mathf.RoundToInt(shieldAbsorbInt / Mathf.Max(0.0001f, shieldDamageAbsorb));
            if (remainder > 0)
            {
                currentHealth -= remainder;
            }
        }
        else
        {
            currentHealth -= amount;
        }

        lastDamageTime = Time.time;
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(ShieldRegenDelayCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Aplicar daño que ignora el escudo (por ejemplo, ataques muy fuertes)
    public void ApplyDirectDamage(int amount)
    {
        if (isInvulnerable) return;
        currentHealth -= amount;
        lastDamageTime = Time.time;
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(ShieldRegenDelayCoroutine());
        if (currentHealth <= 0) Die();
    }

    IEnumerator ShieldRegenDelayCoroutine()
    {
        yield return new WaitForSeconds(shieldRegenDelay);
        // regeneración continua
        while (currentShield < maxShield)
        {
            currentShield = Mathf.Min(maxShield, currentShield + Mathf.CeilToInt(shieldRegenRate * Time.deltaTime));
            yield return null;
        }
    }

    IEnumerator InvulnerabilityFlash(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void AddShield(int amount)
    {
        currentShield = Mathf.Min(maxShield, currentShield + amount);
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        // acá podes spawnear anim, sonido o reiniciar
        Destroy(gameObject);
    }
}
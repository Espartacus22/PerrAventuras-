using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    public EnemyType enemyData;
    [SerializeField] private Slider healthSlider;

    private int currentHealth;

    void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyStats: Falta EnemyType!");
            return;
        }

        currentHealth = enemyData.maxHealth;
        Debug.Log($"{gameObject.name} spawn con {currentHealth} HP");

        if (healthSlider != null)
        {
            healthSlider.maxValue = enemyData.maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} daño. HP: {currentHealth}");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int GetCurrentHealth() => currentHealth;

    Vector3 GetOrbSpawnPosition()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 5f))
        {
            return hit.point + Vector3.up * 0.5f;
        }
        return transform.position + Vector3.up * 1f;
    }

    void Die()
    {
        if (enemyData.dropPrefab != null)
        {
            Vector3 dropPosition = GetOrbSpawnPosition();
            GameObject drop = Instantiate(enemyData.dropPrefab, dropPosition, Quaternion.identity);

            XPOrb orb = drop.GetComponent<XPOrb>();
            if (orb != null)
                orb.xpAmount = enemyData.xpReward;
        }

        Destroy(gameObject);
    }
}
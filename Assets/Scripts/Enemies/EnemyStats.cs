using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyType enemyData;
    private int currentHealth;

    void Start()
    {
        currentHealth = enemyData.maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }
    Vector3 GetOrbSpawnPosition()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 5f))
        {
            return hit.point + Vector3.up * 0.5f; // orb justo encima del suelo
        }

        return transform.position + Vector3.up * 1f; // fallback
    }

    void Die()
    {
        if (enemyData.dropPrefab != null)
        {
           Vector3 dropPosition = new Vector3(transform.position.x, 0.2f, transform.position.z);

            GameObject drop = Instantiate(enemyData.dropPrefab, dropPosition, Quaternion.identity);

            XPOrb orb = drop.GetComponent<XPOrb>();
            if (orb != null)
                orb.xpAmount = enemyData.xpReward;
        }

        Destroy(gameObject);
    }
}
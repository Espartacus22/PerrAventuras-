using UnityEngine;

public class BreakableChest : MonoBehaviour
{
    public int maxHP = 30;
    int currentHP;

    [Header("Drops")]
    public GameObject xpDropPrefab;     // XPOrb u otro
    public int xpDropCount = 3;

    public GameObject healthDropPrefab; // HealthPickup, opcional
    public int healthDropCount = 1;

    void Start()
    {
        currentHP = maxHP;
        GetComponent<Collider>().isTrigger = false; // puede ser sólido
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
            Break();
    }

    void Break()
    {
        // XP
        for (int i = 0; i < xpDropCount; i++)
        {
            if (xpDropPrefab != null)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * 0.5f;
                pos.y = transform.position.y + 0.5f;
                Instantiate(xpDropPrefab, pos, Quaternion.identity);
            }
        }

        // Health
        for (int i = 0; i < healthDropCount; i++)
        {
            if (healthDropPrefab != null)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * 0.5f;
                pos.y = transform.position.y + 0.5f;
                Instantiate(healthDropPrefab, pos, Quaternion.identity);
            }
        }

        // TODO: partículas, sonido
        Destroy(gameObject);
    }
}

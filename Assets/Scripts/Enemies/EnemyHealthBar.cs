using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    private EnemyStats enemyStats;

    void Start()
    {
        enemyStats = GetComponentInParent<EnemyStats>();

        if (enemyStats != null && healthSlider != null && enemyStats.enemyData != null)
        {
            healthSlider.maxValue = enemyStats.enemyData.maxHealth;
            healthSlider.value = enemyStats.enemyData.maxHealth;
        }
    }

    void Update()
    {
        if (enemyStats == null || healthSlider == null)
            return;

        if (!enemyStats.IsReady())
            return;

        healthSlider.value = enemyStats.GetCurrentHealth();

        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}


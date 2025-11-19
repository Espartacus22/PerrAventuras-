using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    private EnemyStats enemyStats;

    void Start()
    {
        enemyStats = GetComponentInParent<EnemyStats>();
        healthSlider.maxValue = enemyStats.enemyData.maxHealth;
        healthSlider.value = enemyStats.enemyData.maxHealth;
    }

    void Update()
    {
        if (enemyStats != null)
        {
            healthSlider.value = enemyStats.GetCurrentHealth();
            transform.LookAt(Camera.main.transform);
        }
    }
}


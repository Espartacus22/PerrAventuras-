using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float speed = 10f;

    private float damage;
    private float maxRange;
    private Vector3 startPosition;

    public void SetRange(float range)
    {
        maxRange = range;
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled > maxRange)
        {
          
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1) Ignorar al player (para no auto-daños si el collider del arma está cerca)
        if (other.CompareTag("Player"))
            return;

        // 2) Enemigos (EnemyStats)
        EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            Debug.Log($"Impactó a ENEMIGO: {enemy.name} con {damage} de daño.");
            enemy.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
            return;
        }

        // 3) Núcleo de energía de torretas
        EnergyCore core = other.GetComponentInParent<EnergyCore>();
        if (core != null)
        {
            core.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
            return;
        }

        // 4) Cofre destructible
        BreakableChest chest = other.GetComponentInParent<BreakableChest>();
        if (chest != null)
        {
            Debug.Log($"Impactó al COFRE: {chest.name} con {damage} de daño.");
            chest.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
            return;
        }

        // 5) Cualquier otra cosa: destruir el proyectil
        Destroy(gameObject);
    }
}


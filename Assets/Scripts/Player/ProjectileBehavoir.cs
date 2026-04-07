using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float speed = 10f;
    public int coreDamage = 10;      // daño al núcleo de energía

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

        Debug.Log("Projectile hit: " + other.name);
        // 1) Ignorar al player
        if (other.CompareTag("Player"))
            return;

        // 2) Núcleo de energía (PRIMERO)
        EnergyCore core = other.GetComponentInParent<EnergyCore>();
        if (core != null)
        {

            // Opcional: si el núcleo también tiene EnemyStats (para barra de vida, etc.)
            EnemyStats coreStats = core.GetComponent<EnemyStats>();
            if (coreStats != null)
            {
                coreStats.TakeDamage(Mathf.RoundToInt(damage));
            }

            Destroy(gameObject);
            return;
        }

        // 3) Enemigos genéricos
        EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            Debug.Log($"Impactó a ENEMIGO: {enemy.name} con {damage} de daño.");
            enemy.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
            return;
        }

        // 4) Cofre destructible
        Breakable breakable = other.GetComponentInParent<Breakable>();

        if (breakable != null)
        {
            Debug.Log($"Impactó al BREAKABLE: {breakable.name} con {damage} de daño.");
            breakable.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
            return;
        }

        // 5) Cualquier otra cosa
        Destroy(gameObject);
    }
}


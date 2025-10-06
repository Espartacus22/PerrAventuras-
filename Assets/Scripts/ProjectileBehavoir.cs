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
            Debug.Log("Proyectil destruido por superar el rango.");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Impact� a: {other.name} con {damage} de da�o.");
            // Aplicar da�o si ten�s un sistema de salud
            // other.GetComponent<EnemyHealth>()?.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}


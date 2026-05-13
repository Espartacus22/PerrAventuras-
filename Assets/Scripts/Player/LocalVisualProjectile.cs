using UnityEngine;

public class LocalVisualProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 3f;

    private float maxRange;
    private Vector3 startPosition;

    // Método para que la estrategia le asigne el rango
    public void SetRange(float range)
    {
        maxRange = range;
    }

    void Start()
    {
        startPosition = transform.position;
        // Destrucción por tiempo como respaldo (seguridad)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // --- LIMITACIÓN POR RANGO ---
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled > maxRange)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}

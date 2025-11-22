using UnityEngine;

public class PaintProjectileBehavior : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 4f;
    public int damage = 20;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Dañar al jugador
        if (other.CompareTag("Player"))
        {
            // Cambiá estos nombres por tu script real de vida del jugador
            var hp1 = other.GetComponent<PlayerLevel>();
            if (hp1 != null) hp1.TakeDamage(damage);

            var hp2 = other.GetComponent<PlayerLevel>();
            if (hp2 != null) hp2.TakeDamage(damage);

            Debug.Log($"Lata de pintura impactó al jugador por {damage}");
            Destroy(gameObject);
            return;
        }

        // Opcional: dañar muros de pintura del propio boss
        PaintWall wall = other.GetComponentInParent<PaintWall>();
        if (wall != null)
        {
            wall.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Choca con algo que no nos interesa
        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}

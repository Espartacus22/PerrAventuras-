using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configuraci�n general")]
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;
    public string targetTag = "Player"; // Cambialo a "Enemy" si lo usas para el jugador

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Intentar aplicar da�o si el objetivo tiene PlayerHealthLocal
            var health = other.GetComponent<PlayerHealthLocal>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Si impacta con algo que no es un trigger (pared, suelo, etc.)
            Destroy(gameObject);
        }
    }
}

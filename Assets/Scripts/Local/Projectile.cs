using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configuración del proyectil")]
    public float speed = 25f;
    public float lifetime = 5f;
    public int damage = 1;
    public string targetTag = "Enemy"; // Por defecto impacta contra enemigos

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"{name}: Falta Rigidbody en el proyectil.");
            return;
        }

        // Desactiva la gravedad para un movimiento recto
        rb.useGravity = false;

        // Dirección inicial (avanza hacia adelante)
        rb.linearVelocity = transform.forward * speed;

        // Destruye el proyectil tras su tiempo de vida
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Evita autocolisión con quien lo disparó
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"{name} impactó contra {targetTag}");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Permite configurar dinámicamente a quién debe atacar (Player o Enemy)
    /// </summary>
    public void SetTargetTag(string newTag)
    {
        targetTag = newTag;
    }

    /// <summary>
    /// Define dirección de disparo personalizada (útil para disparos dirigidos)
    /// </summary>
    public void SetDirection(Vector3 dir)
    {
        if (rb != null)
        {
            rb.linearVelocity = dir.normalized * speed;
        }
    }
}

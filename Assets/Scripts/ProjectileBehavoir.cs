using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float lifetime = 5f;
    public string targetTag = "Enemy";

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return; // evita chocar con el jugador

        if (other.CompareTag(targetTag))
        {
            Debug.Log("Impacto con " + other.name);
            // Futuro: aplicar daño
        }

        Destroy(gameObject);
    }
}


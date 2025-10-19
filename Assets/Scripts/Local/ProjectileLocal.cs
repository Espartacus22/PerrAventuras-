using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ProjectileLocal: Maneja proyectiles en modo offline.
/// Se instancia con Instantiate y destruye al colisionar.
/// </summary>
public class ProjectileLocal : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 10f;
    public int damage = 10;
    public float lifetime = 5f;

    private Vector3 direction;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthLocal health = other.GetComponent<PlayerHealthLocal>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}

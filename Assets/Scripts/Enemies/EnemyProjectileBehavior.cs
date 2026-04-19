using UnityEngine;
using Fusion; // ¡Añadido!

public class EnemyProjectileBehavior : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    public float speed = 15f;

    // Variables sincronizadas
    [Networked] private float damage { get; set; }
    [Networked] private float maxRange { get; set; } = 50f; // Le damos un rango máximo por seguridad
    [Networked] private Vector3 startPosition { get; set; }

    public void SetRange(float range) { maxRange = range; }
    public void SetDamage(float value) { damage = value; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            startPosition = transform.position;
        }
    }

    // Cambiamos Update por FixedUpdateNetwork para que vuele al ritmo del servidor
    public override void FixedUpdateNetwork()
    {
        // Todos los clientes mueven la bala visualmente
        transform.Translate(Vector3.forward * speed * Runner.DeltaTime);

        // Solo el servidor verifica si voló demasiado lejos para destruirla
        if (HasStateAuthority)
        {
            float distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled > maxRange)
            {
                Runner.Despawn(Object); // Despawn en lugar de Destroy
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // CANDADO DE RED: Solo el servidor procesa las colisiones y el daño
        if (!HasStateAuthority) return;

        // Daño al player
        if (other.CompareTag("Player"))
        {
            PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.TakeDamage(Mathf.RoundToInt(damage));
            }

            Runner.Despawn(Object);
            return;
        }
        // Ignoramos a otros enemigos, pero chocamos con muros, piso, etc.
        else if (!other.CompareTag("Enemy"))
        {
            Runner.Despawn(Object);
        }
    }
}

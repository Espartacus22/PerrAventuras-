using UnityEngine;
using Fusion; // ¡Añadido!

public class ProjectileBehavior : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    public float speed = 10f;
    public int coreDamage = 10;

    // Variables sincronizadas para que todos sepan cuánto daño hace
    [Networked] private float damage { get; set; }
    [Networked] private float maxRange { get; set; }
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

    public override void FixedUpdateNetwork()
    {
        // Todos mueven la bala visualmente
        transform.Translate(Vector3.forward * speed * Runner.DeltaTime);

        // Solo el servidor la destruye cuando llega a su rango máximo
        if (HasStateAuthority)
        {
            float distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled > maxRange)
            {
                Runner.Despawn(Object); // Usamos Despawn en lugar de Destroy
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // CANDADO ANTI-DOBLE GOLPE: Solo el servidor procesa el daño
        if (!HasStateAuthority) return;

        if (other.CompareTag("Player")) return;

        EnergyCore core = other.GetComponentInParent<EnergyCore>();
        if (core != null)
        {
            EnemyStats coreStats = core.GetComponent<EnemyStats>();
            if (coreStats != null) coreStats.TakeDamage(Mathf.RoundToInt(damage));
            Runner.Despawn(Object);
            return;
        }

        EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            enemy.TakeDamage(Mathf.RoundToInt(damage));
            Runner.Despawn(Object);
            return;
        }

        Breakable breakable = other.GetComponentInParent<Breakable>();
        if (breakable != null)
        {
            breakable.TakeDamage(Mathf.RoundToInt(damage));
            Runner.Despawn(Object);
            return;
        }

        // Si choca contra la pared
        Runner.Despawn(Object);
    }
}


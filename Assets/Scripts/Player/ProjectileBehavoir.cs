using UnityEngine;
using Fusion;

public class ProjectileBehavior : NetworkBehaviour
{
    public float speed = 15f;
    public int coreDamage = 10;

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

        if (HasInputAuthority && !HasStateAuthority)
        {
            Renderer[] todosLosGraficos = GetComponentsInChildren<Renderer>();
            foreach (Renderer grafico in todosLosGraficos)
            {
                grafico.enabled = false;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        transform.Translate(Vector3.forward * speed * Runner.DeltaTime);

        if (HasStateAuthority)
        {
            float distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled > maxRange)
            {
                Runner.Despawn(Object);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (other.CompareTag("Player")) return;

        // --- RESTAURADO TU CÓDIGO DE DAÑO EXACTO ---
        BreakableChest chest = other.GetComponentInParent<BreakableChest>();
        if (chest != null)
        {
            chest.TakeDamage(Mathf.RoundToInt(damage));
            Runner.Despawn(Object);
            return;
        }

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

        Runner.Despawn(Object);
    }
}
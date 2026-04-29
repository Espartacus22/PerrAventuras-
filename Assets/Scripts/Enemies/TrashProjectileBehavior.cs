using UnityEngine;
using Fusion; // ¡Añadido para el multijugador!

public class TrashProjectileBehavior : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;

    // Lo hacemos Networked por si quieres variar el daño de la basura más adelante
    [Networked] public int damage { get; set; } = 10;

    // Reloj oficial del servidor para saber cuándo destruir la basura
    [Networked] private TickTimer lifeTimer { get; set; }

    public override void Spawned()
    {
        // Solo el servidor inicializa el reloj de destrucción
        if (HasStateAuthority)
        {
            lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 1. Todos mueven la basura visualmente al ritmo del servidor
        transform.Translate(Vector3.forward * speed * Runner.DeltaTime);

        // 2. Solo el servidor controla si ya se le acabó el tiempo de vida
        if (HasStateAuthority)
        {
            if (lifeTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
                return; // Cortamos aquí para no procesar nada más
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // CANDADO DE RED: Solo el servidor calcula colisiones para no restar vida x2
        if (!HasStateAuthority) return;

        if (other.CompareTag("Player"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Usamos Despawn oficial en vez de Destroy local
            Runner.Despawn(Object);
            return;
        }

        // choca con el mundo o algo más -> destruir
        if (!other.CompareTag("Enemy"))
        {
            Runner.Despawn(Object);
        }
    }
}

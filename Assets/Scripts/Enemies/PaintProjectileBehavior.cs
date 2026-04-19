using UnityEngine;
using Fusion; // ¡Añadido para multijugador!

public class PaintProjectileBehavior : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    public float speed = 15f;
    public float lifeTime = 4f;

    // Lo hacemos Networked por si el Boss decide cambiar el daño dinámicamente
    [Networked] public int damage { get; set; } = 20;

    // Reloj oficial del servidor para saber cuándo destruir la lata
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
        // 1. Todos mueven la lata visualmente al mismo tiempo
        transform.Translate(Vector3.forward * speed * Runner.DeltaTime);

        // 2. Solo el servidor controla si ya se le acabó el tiempo de vida
        if (HasStateAuthority)
        {
            if (lifeTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
                return; // Cortamos aquí para que no siga ejecutando código
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // CANDADO DE RED: Solo el servidor procesa colisiones para evitar doble daño
        if (!HasStateAuthority) return;

        // Dañar al jugador
        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<PlayerLevel>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log($"Lata de pintura impactó al jugador por {damage}");
            }

            Runner.Despawn(Object);
            return;
        }

        // Opcional: dañar muros de pintura del propio boss
        PaintWall wall = other.GetComponentInParent<PaintWall>();
        if (wall != null)
        {
            wall.TakeDamage(damage);
            Runner.Despawn(Object);
            return;
        }

        // Choca con el mapa u otra cosa que no sea enemigo
        if (!other.CompareTag("Enemy"))
        {
            Runner.Despawn(Object);
        }
    }
}
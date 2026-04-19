using UnityEngine;
using Fusion; // ¡Añadido para multijugador!

public class PaintWall : NetworkBehaviour // Cambiamos a NetworkBehaviour
{
    public int maxHP = 30;
    public float lifeTime = 15f; // si querés que desaparezca solo

    // Sincronizamos la vida para que todos vean la pared romperse al mismo tiempo
    [Networked] private int currentHP { get; set; }

    // Reloj oficial del servidor
    [Networked] private TickTimer lifeTimer { get; set; }

    public override void Spawned()
    {
        // Solo el servidor inicializa la vida y el reloj
        if (HasStateAuthority)
        {
            currentHP = maxHP;

            if (lifeTime > 0)
            {
                lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Solo el servidor controla cuándo se acaba el tiempo de vida
        if (HasStateAuthority)
        {
            if (lifeTime > 0 && lifeTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        // CANDADO DE RED: Solo el servidor procesa el daño
        if (!HasStateAuthority) return;

        currentHP -= amount;

        if (currentHP <= 0)
        {
            // En lugar de Destroy, usamos Despawn para que desaparezca en toda la red
            Runner.Despawn(Object);
        }
    }
}
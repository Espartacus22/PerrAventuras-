using UnityEngine;
using Fusion; // ¡Añadido para red!

public class XPOrb : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    // Sincronizamos la cantidad de XP por si acaso, aunque el servidor es quien la maneja
    [Networked] public int xpAmount { get; set; } = 10;

    public float attractRadius = 5f;
    public float attractSpeed = 8f;

    private Transform targetPlayer;

    // Cambiamos Update por FixedUpdateNetwork
    public override void FixedUpdateNetwork()
    {
        // CANDADO 1: Solo el servidor decide hacia quién vuela el orbe.
        // Así evitamos que en la pantalla del cliente 1 el orbe vuele hacia él, 
        // y en la del cliente 2 vuele hacia el otro. Todos ven lo mismo.
        if (!HasStateAuthority) return;

        if (targetPlayer == null)
        {
            // Buscar al jugador más cercano en el radio
            Collider[] hits = Physics.OverlapSphere(transform.position, attractRadius);
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        targetPlayer = hit.transform;
                    }
                }
            }
        }
        else
        {
            // El servidor mueve el orbe hacia el jugador
            Vector3 dir = (targetPlayer.position - transform.position).normalized;
            transform.position += dir * attractSpeed * Runner.DeltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // CANDADO 2 (EL MÁS IMPORTANTE): Solo el servidor procesa quién lo agarró
        // Esto impide que dos jugadores cobren la misma experiencia
        if (!HasStateAuthority) return;

        if (!other.CompareTag("Player")) return;

        PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.GainXP(xpAmount);

            // El servidor destruye el orbe de manera oficial en todas las pantallas
            Runner.Despawn(Object);
        }
    }
}

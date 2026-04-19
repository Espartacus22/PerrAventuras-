using UnityEngine;
using Fusion; // ¡Añadido para el multijugador!

public class TrashDroneEnemy : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float moveSpeed = 4f;
    public float pointTolerance = 0.5f;
    public float hoverHeight = 3f;

    [Header("Disparo")]
    public Transform[] firePoints;          // dos extremos del cilindro
    public GameObject trashProjectilePrefab;
    public float shootRange = 15f;
    public float shootCooldown = 1.2f;

    // Agregamos el daño aquí para poder pasárselo a la basura al instanciarla
    public int projectileDamage = 10;

    Transform targetPlayer;
    int currentPoint;
    float nextShootTime;

    public override void Spawned()
    {
        // Solo el servidor inicializa la posición oficial
        if (HasStateAuthority)
        {
            FindClosestPlayer();

            // Aseguramos que el dron arranque flotando
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, hoverHeight, pos.z);
        }
    }

    // Cambiamos Update por FixedUpdateNetwork
    public override void FixedUpdateNetwork()
    {
        // REGLA DE ORO: Solo el servidor mueve al dron y efectúa los disparos
        if (!HasStateAuthority) return;

        // Si el jugador objetivo se desconectó o murió, buscar a otro
        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy)
        {
            FindClosestPlayer();
        }

        HandlePatrol();
        HandleShooting();
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = float.MaxValue;
        Transform bestTarget = null;

        foreach (var p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                bestTarget = p.transform;
            }
        }
        targetPlayer = bestTarget;
    }

    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform point = patrolPoints[currentPoint];
        if (point == null) return;

        Vector3 targetPos = new Vector3(point.position.x, hoverHeight, point.position.z);

        // Usamos Runner.DeltaTime para que se mueva suavemente en la red
        transform.position = Vector3.MoveTowards(transform.position,
                                                 targetPos,
                                                 moveSpeed * Runner.DeltaTime);

        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist <= pointTolerance)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
    }

    void HandleShooting()
    {
        if (targetPlayer == null) return;

        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        if (dist > shootRange) return;
        if (Runner.SimulationTime < nextShootTime) return;

        // --- EL AJUSTE DE PUNTERÍA ---
        // Calculamos la dirección real (incluyendo la altura)
        // Le sumamos Vector3.up * 1.2f para que apunte al pecho del player y no a los pies
        Vector3 targetCenter = targetPlayer.position + Vector3.up * 1.2f;
        Vector3 dirToTarget = (targetCenter - transform.position).normalized;

        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            // El dron entero rota para mirar al jugador
            transform.rotation = Quaternion.LookRotation(dirToTarget);
        }

        // Disparar desde todos los firePoints
        if (trashProjectilePrefab != null && firePoints != null)
        {
            NetworkObject netPrefab = trashProjectilePrefab.GetComponent<NetworkObject>();
            if (netPrefab != null)
            {
                foreach (var fp in firePoints)
                {
                    if (fp == null) continue;
                    // Ahora fp.rotation ya está inclinado hacia abajo porque el dron rotó
                    Runner.Spawn(netPrefab, fp.position, fp.rotation);
                }
            }
        }

        nextShootTime = Runner.SimulationTime + shootCooldown;
    }
}
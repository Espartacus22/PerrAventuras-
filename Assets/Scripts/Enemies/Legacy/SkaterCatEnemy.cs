using UnityEngine;
using UnityEngine.AI;
using Fusion; // ¡Añadido para el multijugador!

public class SkaterCatEnemy : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    [Header("Jugador")]
    public Transform target;

    [Header("Datos generales")]
    public EnemyType enemyData;
    private EnemyStats stats;
    private NavMeshAgent agent;

    [Header("Patrulla")]
    [Tooltip("Puntos de patrulla alrededor de la fuente (nivel 2 + nivel 3)")]
    public Transform[] patrolPoints;
    public float pointTolerance = 0.3f;

    private int currentPatrolIndex = 0;

    [Header("Disparo")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Sniper (rango largo)")]
    public float sniperRange = 25f;
    public float sniperCooldown = 1.5f;
    public int sniperDamage = 20;

    [Header("Furia metralleta (rango corto)")]
    public float furyRange = 8f;
    public float furyFireRate = 0.2f;     // disparos por segundo
    public int furyDamage = 30;
    public float furyWindupTime = 2f;     // segundos de aviso antes de empezar

    // Variables de tiempo actualizadas para la red
    private float nextShootTime;
    private bool furyCharging;
    private bool furyActive;
    private float furyStartTime;
    
    private float lastTargetSearchTime;
    [SerializeField] private float targetSearchInterval = 0.5f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<EnemyStats>();
    }

    public override void Spawned()
    {
        // Solo el servidor controla el NavMesh y las stats base
        if (!HasStateAuthority)
        {
            if (agent != null) agent.enabled = false;
            return;
        }

        if (enemyData != null && agent != null)
        {
            agent.speed = enemyData.moveSpeed;
            if (stats != null) stats.enemyData = enemyData;
        }

        FindClosestPlayer();

        if (patrolPoints != null && patrolPoints.Length > 0 && agent != null)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    // Cambiado Update por FixedUpdateNetwork
    public override void FixedUpdateNetwork()
    {
        // REGLA DE ORO: Solo el servidor piensa, se mueve y dispara
        if (!HasStateAuthority) return;

        if (Runner.SimulationTime >= lastTargetSearchTime + targetSearchInterval)
        {
            FindClosestPlayer();
            lastTargetSearchTime = Runner.SimulationTime;
        }

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindClosestPlayer();
            if (target == null)
            {
                Patrol();
                return;
            }
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // --- Rango furia ---
        if (distance <= furyRange)
        {
            HandleFury();
        }
        // --- Rango sniper ---
        else if (distance <= sniperRange)
        {
            ResetFury();
            HandleSniper();
        }
        // --- Fuera de rango: patrulla ---
        else
        {
            ResetFury();
            Patrol();
        }
    }

    void FindClosestPlayer()
    {
        NetPlayerDamageAdapter[] players =
        FindObjectsByType<NetPlayerDamageAdapter>(FindObjectsSortMode.None);

        float closestDist = float.MaxValue;
        Transform bestTarget = null;

        foreach (var p in players)
        {
            if (p == null) continue;

            float d = Vector3.Distance(transform.position, p.transform.position);

            if (d < closestDist)
            {
                closestDist = d;
                bestTarget = p.transform;
            }
        }

        target = bestTarget;
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || agent == null) return;

        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= pointTolerance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void HandleSniper()
    {
        agent.isStopped = true;
        LookAtTarget();

        // Usamos Runner.SimulationTime en lugar de Time.time
        if (Runner.SimulationTime >= nextShootTime)
        {
            Shoot(sniperDamage, sniperRange);
            nextShootTime = Runner.SimulationTime + sniperCooldown;
        }
    }

    void HandleFury()
    {
        agent.isStopped = true;
        LookAtTarget();

        // primer ingreso al rango corto -> empezamos carga
        if (!furyCharging && !furyActive)
        {
            furyCharging = true;
            furyStartTime = Runner.SimulationTime; // Actualizado a red
        }

        // después de los 2s de ventaja, pasamos a modo activo
        if (furyCharging && Runner.SimulationTime >= furyStartTime + furyWindupTime)
        {
            furyCharging = false;
            furyActive = true;
        }

        // mientras esté activo, dispara ráfagas
        if (furyActive && Runner.SimulationTime >= nextShootTime)
        {
            Shoot(furyDamage, furyRange);
            nextShootTime = Runner.SimulationTime + furyFireRate; // Ajustado cálculo de fireRate
        }
    }

    void ResetFury()
    {
        furyCharging = false;
        furyActive = false;
    }

    void LookAtTarget()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRot = Quaternion.LookRotation(dir);
            // Usamos Runner.DeltaTime para rotar suavemente
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 10f * Runner.DeltaTime);
        }
    }

    void Shoot(int damage, float range)
    {
        if (projectilePrefab == null || firePoint == null || target == null)
            return;

        Vector3 dir = (target.position + Vector3.up * 1.2f - firePoint.position).normalized;

        // Extraemos el componente de red del prefab de la bala
        NetworkObject netPrefab = projectilePrefab.GetComponent<NetworkObject>();

        if (netPrefab != null)
        {
            // Disparo oficial por la red
            NetworkObject go = Runner.Spawn(netPrefab, firePoint.position, Quaternion.LookRotation(dir));

            EnemyProjectileBehavior proj = go.GetComponent<EnemyProjectileBehavior>();
            if (proj != null)
            {
                proj.SetDamage(damage);
                proj.SetRange(range);
            }
        }
        else
        {
            Debug.LogWarning($"¡El prefab del proyectil del {gameObject.name} necesita un NetworkObject!");
        }
    }
}

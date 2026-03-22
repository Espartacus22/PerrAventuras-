using UnityEngine;
using UnityEngine.AI;

public class SkaterCatEnemy : MonoBehaviour
{
    [Header("Jugador")]
    public Transform target;              // se auto-asigna por tag si lo dejas vacío

    [Header("Datos generales")]
    public EnemyType enemyData;           // usarás uno específico de SkaterCat
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

    private float nextShootTime;
    private bool furyCharging;
    private bool furyActive;
    private float furyStartTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<EnemyStats>();

        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            stats.enemyData = enemyData; // asegura mismo SO
        }
    }

    void Start()
    {
        if (target == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                target = playerGO.transform;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if (target == null)
        {
            Patrol();
            return;
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

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

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

        if (Time.time >= nextShootTime)
        {
            Shoot(sniperDamage, sniperRange);
            nextShootTime = Time.time + sniperCooldown;
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
            furyStartTime = Time.time;
            // acá podrías disparar animación/FX de aviso
        }

        // después de los 3s de ventaja, pasamos a modo activo
        if (furyCharging && Time.time >= furyStartTime + furyWindupTime)
        {
            furyCharging = false;
            furyActive = true;
        }

        // mientras esté activo, dispara ráfagas
        if (furyActive && Time.time >= nextShootTime)
        {
            Shoot(furyDamage, furyRange);
            nextShootTime = Time.time + (1f / furyFireRate);
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
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 10f * Time.deltaTime);
        }
    }

    void Shoot(int damage, float range)
    {
        if (projectilePrefab == null || firePoint == null || target == null)
            return;

        Vector3 dir = (target.position + Vector3.up * 1.2f - firePoint.position).normalized;
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        EnemyProjectileBehavior proj = go.GetComponent<EnemyProjectileBehavior>();
        if (proj != null)
        {
            proj.SetDamage(damage);
            proj.SetRange(range);
        }
    }
}

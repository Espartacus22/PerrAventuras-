using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyFollow : MonoBehaviour
{
    [Header("Jugador")]
    public Transform target;
    public string playerTag = "Player";

    [Header("Datos generales")]
    public EnemyType enemyData;
    private EnemyStats stats;
    private NavMeshAgent agent;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float pointTolerance = 0.3f;
    private int currentPatrolIndex = 0;

    [Header("Disparo")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Sniper (rango largo)")]
    public float sniperRange = 50f;
    public float sniperCooldown = 1.5f;
    public int sniperDamage = 20;

    [Header("Furia metralleta (rango corto)")]
    public float furyRange = 8f;
    public float furyFireRate = 5f;
    public int furyDamage = 30;
    public float furyWindupTime = 2f;

    [Header("Retarget")]
    public float retargetInterval = 0.35f;
    private float nextRetargetTime;

    private float nextShootTime;

    private Transform lockedTarget;
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
            stats.enemyData = enemyData;
        }
    }

    void Start()
    {
        AcquireTarget();
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        if (Time.time >= nextRetargetTime && lockedTarget == null)
        {
            AcquireTarget();
            nextRetargetTime = Time.time + retargetInterval;
        }

        if (lockedTarget != null)
            target = lockedTarget;

        if (target == null)
        {
            ResetFury();
            Patrol();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= furyRange)
        {
            if (lockedTarget == null) lockedTarget = target;
            HandleFury();
        }
        else if (distance <= sniperRange)
        {
            lockedTarget = null;
            ResetFury();
            HandleSniper();
        }
        else
        {
            lockedTarget = null;
            ResetFury();
            Patrol();
        }
    }

    void AcquireTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            target = playerObj.transform;
        else
            target = null;
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
            Shoot(sniperDamage);
            nextShootTime = Time.time + sniperCooldown;
        }
    }

    void HandleFury()
    {
        agent.isStopped = true;
        LookAtTarget();

        if (!furyCharging && !furyActive)
        {
            furyCharging = true;
            furyStartTime = Time.time;
        }

        if (furyCharging && Time.time >= furyStartTime + furyWindupTime)
        {
            furyCharging = false;
            furyActive = true;
        }

        if (!furyActive) return;

        float interval = 1f / Mathf.Max(0.1f, furyFireRate);
        if (Time.time >= nextShootTime)
        {
            Shoot(furyDamage);
            nextShootTime = Time.time + interval;
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

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 10f * Time.deltaTime);
        }
    }

    void Shoot(int damage)
    {
        if (projectilePrefab == null || firePoint == null || target == null) return;

        Vector3 dir = (target.position + Vector3.up * 1.2f - firePoint.position).normalized;
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        var proj = go.GetComponent<PaintProjectileBehavior>();
        if (proj != null)
        {
            proj.damage = damage;
        }
    }
}
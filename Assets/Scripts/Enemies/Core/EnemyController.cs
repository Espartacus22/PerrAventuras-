using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyController : NetworkBehaviour
{
    [Header("Data")]
    public EnemyType enemyData;

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    public Transform Target { get; private set; }

    [Header("Ranges")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float pointTolerance = 0.3f;
    public int currentPatrolIndex = 0;

    [Header("Attack")]
    public int attackDamage = 10;

    [Header("Projectile Attack")]
    [SerializeField] private bool useProjectileAttack = false;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 12f;

    public GameObject ProjectilePrefab => projectilePrefab;
    public Transform FirePoint => firePoint;
    public float ProjectileSpeed => projectileSpeed;

    public EStateMachine StateMachine { get; private set; }
    public EIdleState IdleState { get; private set; }
    public EPatrolState PatrolState { get; private set; }
    public EChaseState ChaseState { get; private set; }
    public EAttackState AttackState { get; private set; }

    public NavMeshAgent Agent { get; private set; }
    public EnemyStats Stats { get; private set; }

    public float LastAttackTime { get; set; }

    private IEnemyAttackStrategy attackStrategy;

    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<EnemyStats>();

        if (enemyData != null)
        {
            Agent.speed = enemyData.moveSpeed;
            Stats.enemyData = enemyData;
        }

        StateMachine = new EStateMachine();
        IdleState = new EIdleState(this, StateMachine);
        PatrolState = new EPatrolState(this, StateMachine);
        ChaseState = new EChaseState(this, StateMachine);
        AttackState = new EAttackState(this, StateMachine);

        if (useProjectileAttack)
            attackStrategy = new ProjectileEnemyAttackStrategy();
        else
            attackStrategy = new MeleeEnemyAttackStrategy();
    }

    // CAMBIO MULTIJUGADOR: Usamos Spawned en lugar de Start
    public override void Spawned()
    {
        // Solo el servidor inicializa la IA
        if (!HasStateAuthority)
        {
            // Apagamos el NavMesh en los clientes para que no pelee contra el NetworkTransform
            Agent.enabled = false;
            return;
        }

        FindTarget();

        if (patrolPoints != null && patrolPoints.Length > 0)
            StateMachine.Initialize(PatrolState);
        else
            StateMachine.Initialize(IdleState);
    }

    // CAMBIO MULTIJUGADOR: Usamos FixedUpdateNetwork en lugar de Update
    public override void FixedUpdateNetwork()
    {
        // REGLA DE ORO: Solo el servidor piensa y mueve a los enemigos
        if (!HasStateAuthority) return;

        // Si perdimos al objetivo (ej. se desconectó o murió), buscamos otro
        if (Target == null || !Target.gameObject.activeInHierarchy)
            FindTarget();

        StateMachine.CurrentState?.LogicUpdate();
    }

    // CAMBIO MULTIJUGADOR: Ahora busca al jugador más cercano
    public void FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
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

        Target = bestTarget;
    }

    public float DistanceToTarget()
    {
        if (Target == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, Target.position);
    }

    public bool IsTargetInDetectionRange()
    {
        return DistanceToTarget() <= detectionRange;
    }

    public bool IsTargetInAttackRange()
    {
        return DistanceToTarget() <= attackRange;
    }

    public void MoveToTarget()
    {
        if (Target == null) return;
        Agent.isStopped = false;
        Agent.SetDestination(Target.position);
    }

    public void StopMoving()
    {
        Agent.isStopped = true;
    }

    public void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Agent.isStopped = false;

        if (!Agent.pathPending && Agent.remainingDistance <= pointTolerance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            Agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    public void LookAtTarget()
    {
        if (Target == null) return;

        Vector3 dir = Target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            // CAMBIO MULTIJUGADOR: Runner.DeltaTime en vez de Time.deltaTime
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Runner.DeltaTime);
        }
    }

    public void Attack()
    {
        // CAMBIO MULTIJUGADOR: Runner.SimulationTime en vez de Time.time
        if (Runner.SimulationTime < LastAttackTime + attackCooldown) return;

        LastAttackTime = Runner.SimulationTime;
        attackStrategy.Execute(this);
    }
}

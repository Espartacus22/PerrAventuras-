using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
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

        attackStrategy = new MeleeEnemyAttackStrategy();
    }

    private void Start()
    {
        FindTarget();

        if (patrolPoints != null && patrolPoints.Length > 0)
            StateMachine.Initialize(PatrolState);
        else
            StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (Target == null)
            FindTarget();

        StateMachine.CurrentState?.LogicUpdate();
    }

    public void FindTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        Target = playerObj != null ? playerObj.transform : null;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    public void Attack()
    {
        if (Time.time < LastAttackTime + attackCooldown) return;

        LastAttackTime = Time.time;
        attackStrategy.Execute(this);
    }
}

using UnityEngine;
using UnityEngine.AI;
using Fusion; // Añadido

public class GraffityCatCloneEnemy : NetworkBehaviour // Cambiado
{
    public EnemyType enemyData;
    public EnemyStats enemyStats;
    public NavMeshAgent agent;
    public Transform playerTarget;

    [Header("Ataque melee (clon)")]
    public float meleeRange = 1.8f;
    public float meleeCooldown = 2f;
    public int meleeDamage = 15;
    public LayerMask playerLayer;

    private GraffityCatEnemy bossOwner;
    private float nextMeleeTime;

    public void InitClone(Transform player, GraffityCatEnemy owner)
    {
        playerTarget = player;
        bossOwner = owner;
    }

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (enemyStats == null) enemyStats = GetComponent<EnemyStats>();
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            if (agent != null) agent.enabled = false;
            return;
        }

        if (enemyData != null && agent != null)
        {
            agent.speed = enemyData.moveSpeed * 1.1f;
            agent.stoppingDistance = meleeRange * 0.7f;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || playerTarget == null || enemyStats == null) return;

        agent.SetDestination(playerTarget.position);
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        if (dist <= meleeRange) TryMelee();
    }

    void TryMelee()
    {
        if (Runner.SimulationTime < nextMeleeTime) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, meleeRange, playerLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(meleeDamage);
            }

            break;
        }

        nextMeleeTime = Runner.SimulationTime + meleeCooldown;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (bossOwner != null) bossOwner.NotifyCloneDead(this);
    }
}

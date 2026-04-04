using UnityEngine;
using UnityEngine.AI;

public class GraffityCatCloneEnemy : MonoBehaviour
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

    void Start()
    {
        if (enemyData != null && agent != null)
        {
            agent.speed = enemyData.moveSpeed * 1.1f; // un poco más rápido que minion normal
            agent.stoppingDistance = meleeRange * 0.7f;
        }

        if (playerTarget == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerTarget = playerGO.transform;
        }
    }

    void Update()
    {
        if (playerTarget == null || enemyStats == null) return;

        agent.SetDestination(playerTarget.position);

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist <= meleeRange)
        {
            TryMelee();
        }
    }

    void TryMelee()
    {
        if (Time.time < nextMeleeTime) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, meleeRange, playerLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            var hp = hit.GetComponent<PlayerLevel>();
            if (hp != null) hp.TakeDamage(meleeDamage);

            Debug.Log($"Clon de GraffityCat golpeó al jugador por {meleeDamage}");
            break;
        }

        nextMeleeTime = Time.time + meleeCooldown;
    }

    void OnDestroy()
    {
        if (bossOwner != null)
        {
            bossOwner.NotifyCloneDead(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}

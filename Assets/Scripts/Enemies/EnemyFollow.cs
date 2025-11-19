using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyFollow : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyType enemyData;

    [Header("Rangos")]
    public float chaseRange = 15f;
    public float attackRange = 2f;

    [Header("Ataque")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;

    private Transform target;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private float lastPathUpdateTime;
    private float pathUpdateInterval = 0.5f; // Solo actualizar path cada 0.5s

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // CONFIGURACIÓN CLAVE PARA RIGIDBODY PLAYER
        agent.updatePosition = true;
        agent.updateRotation = false; // Nosotros controlamos la rotación
        agent.autoBraking = true; // Frena suavemente

        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            agent.angularSpeed = 360f; // Rota rápido
        }
    }

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        // Buscar jugador si se pierde (importante por respawn)
        if (target == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        // PERSEGUIR (solo si está en rango)
        if (distance <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            agent.ResetPath(); // Para de perseguir
        }

        // ATACAR (si está muy cerca)
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }

        // Rotar hacia jugador (suave)
        RotateToPlayer();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"[{gameObject.name}] Encontró jugador!");
        }
    }

    void ChasePlayer()
    {
        // Actualizar path solo cada 0.5 segundos (OPTIMIZADO)
        if (Time.time >= lastPathUpdateTime + pathUpdateInterval)
        {
            agent.SetDestination(target.position);
            lastPathUpdateTime = Time.time;
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        PlayerLevel playerLevel = target.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.TakeDamage(Mathf.RoundToInt(attackDamage));
        }
    }

    void RotateToPlayer()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // Solo rotar en Y

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    // GIZMOS para debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
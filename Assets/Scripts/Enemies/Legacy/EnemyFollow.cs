using Fusion; // ¡Anadido para el multijugador!
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyFollow : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    [Header("Configuracion")]
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

    private float lastTargetSearchTime;
    private float targetSearchInterval = 0.5f;


    //Inicializa referencias locales del enemigo antes de iniciar la partida.
    //Acá toma el NavMeshAgent y configura velocidad, rotación y frenado.-
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // CONFIGURACIÓN CLAVE PARA RIGIDBODY PLAYER
        agent.updatePosition = true;
        agent.updateRotation = true; // Nosotros controlamos la rotación
        agent.autoBraking = false; // Frena suavemente
        agent.isStopped = false;

        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            agent.angularSpeed = 360f; // Rota rápido
        }
    }

    //Se ejecuta cuando Fusion crea el enemigo en red.
    //Si no tiene autoridad de estado, desactiva el NavMeshAgent.
    //Solo el Host/Server controla la IA.
    public override void Spawned()
    {
        // Solo el servidor controla el NavMesh. Los clientes solo lo ven moverse.
        if (!HasStateAuthority)
        {
            agent.enabled = false;
            return;
        }

        FindPlayer();
    }

    // Loop principal de red.
    // Solo el server decide a quién perseguir, cuando moverse, cuando atacar y hacia dónde rotar.
    public override void FixedUpdateNetwork()
    {
        // REGLA DE ORO: Solo el servidor mueve al enemigo y ejecuta ataques
        if (!Object.HasStateAuthority) return;

        if (Runner.SimulationTime >= lastTargetSearchTime + targetSearchInterval)
        {
            FindPlayer();
            lastTargetSearchTime = Runner.SimulationTime;
        }

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            agent.ResetPath();
        }

        if (distance <= attackRange && Runner.SimulationTime >= lastAttackTime + attackCooldown)
        {
            Attack();
        }

        RotateToPlayer();
    }

    //Busca y asigna el jugador objetivo.
    //Ahora debería usar FindClosestPlayer() para detectar Host o Client.
    void FindPlayer()
    {
        // En multijugador, buscamos al jugador más cercano en lugar de uno al azar
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

        if (target != null)
        {
            Debug.Log($"[{gameObject.name}] Target más cercano: {target.name}");
        }
    }

    //Busca todos los players con NetPlayerDamageAdapter y devuelve el más cercano al enemigo.
    private Transform FindClosestPlayer()
    {
        NetPlayerDamageAdapter[] players = FindObjectsByType<NetPlayerDamageAdapter>(FindObjectsSortMode.None);

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var p in players)
        {
            if (p == null) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        return closest;
    }

    // Actualiza el destino del NavMeshAgent cada cierto intervalo para no recalcular path todo el tiempo.
    void ChasePlayer()
    {
        // Actualizar path usando el reloj del servidor
        if (Runner.SimulationTime >= lastPathUpdateTime + pathUpdateInterval)
        {
            if (!agent.enabled || !agent.isOnNavMesh)
            {
                Debug.LogWarning($"[{gameObject.name}] Agent no válido. Enabled: {agent.enabled}, OnNavMesh: {agent.isOnNavMesh}");
                return;
            }

            agent.isStopped = false;
            agent.speed = enemyData != null ? enemyData.moveSpeed : 3.5f;
            bool pathOk = agent.SetDestination(target.position);

            Debug.Log(
                $"[{gameObject.name}] Persiguiendo a {target.name} | " +
                $"SetDestination: {pathOk} | " +
                $"PathStatus: {agent.pathStatus} | " +
                $"Remaining: {agent.remainingDistance} | " +
                $"Velocity: {agent.velocity}"
            );

            lastPathUpdateTime = Runner.SimulationTime;
        }
    }

    // Aplica daño al target mediante IDamageable.
    // Así el enemigo no depende directo de PlayerLevel.
    void Attack()
    {
        lastAttackTime = Runner.SimulationTime;

        IDamageable damageable = target.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(Mathf.RoundToInt(attackDamage));
            Debug.Log($"[{gameObject.name}] Atacó a {target.name} por {attackDamage}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Target sin IDamageable: {target.name}");
        }
    }

    //Rota suavemente al enemigo hacia el jugador objetivo.
    void RotateToPlayer()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // Solo rotar en Y

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // Usamos Runner.DeltaTime en vez de Time.deltaTime
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Runner.DeltaTime);
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
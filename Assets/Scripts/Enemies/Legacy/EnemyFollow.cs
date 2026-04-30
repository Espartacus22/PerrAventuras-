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


    //Inicializa referencias locales del enemigo antes de iniciar la partida.
    //Acá toma el NavMeshAgent y configura velocidad, rotación y frenado.-
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
        if (!HasStateAuthority) return;

        if (target != null)
        {
            Debug.Log($"[{gameObject.name}] Target actual: {target.name}");
        }

        // Buscar jugador si se pierde (ej. si se desconecta o muere)
        if (target == null || !target.gameObject.activeInHierarchy)
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

        // ATACAR (Usamos Runner.SimulationTime en vez de Time.time)
        if (distance <= attackRange && Runner.SimulationTime >= lastAttackTime + attackCooldown)
        {
            Attack();
        }

        // Rotar hacia jugador (suave)
        RotateToPlayer();
    }

    //Busca y asigna el jugador objetivo.
    //Ahora debería usar FindClosestPlayer() para detectar Host o Client.
    void FindPlayer()
    {
        // En multijugador, buscamos al jugador más cercano en lugar de uno al azar
        target = FindClosestPlayer();

        if (target != null)
        {
            Debug.Log($"[{gameObject.name}] Encontró al jugador más cercano: {target.name}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No encontró jugadores.");
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
            agent.SetDestination(target.position);
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
            // Como esto solo lo ejecuta el servidor, el daño será oficial y no se duplicara
            damageable.TakeDamage(Mathf.RoundToInt(attackDamage));
            Debug.Log($"[{gameObject.name}] Atacó al jugador por {attackDamage}");
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
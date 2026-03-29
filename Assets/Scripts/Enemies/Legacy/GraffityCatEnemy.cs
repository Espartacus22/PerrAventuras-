using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GraffityCatEnemy : MonoBehaviour
{
    public enum BossPhase
    {
        Patrol,
        Combat,
        Enraged
    }
    [SerializeField] private BossPhase currentPhase = BossPhase.Patrol;

    [SerializeField] private bool enableEnragedPhase = true;
    [SerializeField] private float enragedHpThreshold = 0.4f;
    private bool _enragedApplied = false;

    [Header("Datos base")]
    public EnemyType enemyData;          // ScriptableObject del boss
    public EnemyStats enemyStats;        // Mismo componente que usan otros enemigos

    [Header("Refs")]
    public NavMeshAgent agent;
    public Transform playerTarget;       // Se autocompleta por tag Player si se deja vacío

    // ---------------- PATRULLA ----------------
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeedMultiplier = 0.6f;
    public float patrolPointTolerance = 0.3f;

    private int _currentPatrolIndex = 0;
    private bool _hasPatrolRoute = false;

    // Estado de combate
    private bool _inCombat = false;

    [Header("Distancia con el jugador")]
    public float desiredCombatDistance = 2f;

    // ---------------- RANGED ----------------
    [Header("Ataque a distancia (latas de pintura)")]
    public GameObject paintProjectilePrefab;
    public Transform firePoint;          // Boca de disparo
    public float rangedRange = 18f;
    public float rangedCooldown = 2f;
    public int rangedDamage = 20;

    // ---------------- MELEE ----------------
    [Header("Ataque melee fuerte")]
    public float meleeRange = 2.5f;
    public float meleeCooldown = 1.5f;
    public int meleeDamage = 40;
    public Transform meleeOrigin;        // centro del golpe (puede ser el mismo que firePoint)
    public LayerMask playerLayer;

    // ---------------- MUROS ----------------
    [Header("Muros de pintura")]
    public GameObject paintWallPrefab;
    public Transform[] wallSpawnPoints;
    public float wallCooldown = 6f;
    public float wallUseRange = 12f;

    private float _nextWallTime;
    private readonly List<PaintWall> _activeWalls = new List<PaintWall>();

    // ---------------- CLONES ----------------
    [Header("Clones")]
    public GameObject clonePrefab;
    public Transform[] cloneSpawnPoints;
    public int maxClones = 2;
    public float cloneCooldown = 10f;

    private float _nextCloneTime;
    private readonly List<GraffityCatCloneEnemy> _aliveClones = new List<GraffityCatCloneEnemy>();

    // ---------------- SALTO ----------------
    [Header("Salto")]
    public bool enableJump = true;
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;
    public float jumpCooldown = 5f;

    private float _nextJumpTime;
    private bool _isJumping = false;

    // Timers ataques
    private float _nextRangedTime;
    private float _nextMeleeTime;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (enemyStats == null) enemyStats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        if (playerTarget == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                playerTarget = playerGO.transform;
        }

        if (enemyData != null && agent != null)
        {
            agent.speed = enemyData.moveSpeed;
        }

        if (agent != null)
        {
            agent.stoppingDistance = desiredCombatDistance;   // mantiene ~2 unidades
        }

        // Patrulla
        _hasPatrolRoute = patrolPoints != null && patrolPoints.Length > 0;
        _currentPatrolIndex = 0;
    }

    void Update()
    {
        if (playerTarget == null || enemyStats == null) return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);

        bool playerInChaseRange = enemyData != null
            ? dist <= enemyData.chaseRange
            : dist <= rangedRange;

        // Una vez que entra en rango, consideramos que está en combate
        if (playerInChaseRange)
            _inCombat = true;

        HandleMovement(dist, playerInChaseRange);

        if (_inCombat)
        {
            HandleAttacks(dist);
            HandleWalls(dist);
            HandleClones();
            HandleJump(dist);
        }
    }

    void HandleEnragedPhase()
    {
        if (!enableEnragedPhase || _enragedApplied || enemyStats == null || enemyData == null) return;

        float hpPercent = (float)enemyStats.CurrentHealth / enemyData.maxHealth;
        if (hpPercent <= enragedHpThreshold)
        {
            rangedCooldown *= 0.8f;
            meleeCooldown *= 0.8f;
            wallCooldown *= 0.85f;
            _enragedApplied = true;
            currentPhase = BossPhase.Enraged;
            Debug.Log("GraffityCat entró en fase Enraged");
        }
    }

    // ---------------- MOVIMIENTO ----------------
    void HandleMovement(float distToPlayer, bool playerInChaseRange)
    {
        if (agent == null) return;
        if (_isJumping) return;    // mientras está saltando no le damos órdenes nuevas

        // Persecución (mantiene "desiredCombatDistance")
        if (playerInChaseRange)
        {
            agent.isStopped = false;
            if (enemyData != null)
                agent.speed = enemyData.moveSpeed;

            agent.stoppingDistance = desiredCombatDistance;
            agent.SetDestination(playerTarget.position);

            // Mirar hacia el player (solo en XZ)
            Vector3 lookDir = playerTarget.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir);
                transform.rotation = rot;
            }
        }
        // Patrulla
        else if (_hasPatrolRoute)
        {
            agent.isStopped = false;
            if (enemyData != null)
                agent.speed = enemyData.moveSpeed * patrolSpeedMultiplier;

            Transform targetPoint = patrolPoints[_currentPatrolIndex];
            if (targetPoint != null)
            {
                agent.stoppingDistance = 0f;
                agent.SetDestination(targetPoint.position);

                Vector3 lookDir = targetPoint.position - transform.position;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion rot = Quaternion.LookRotation(lookDir);
                    transform.rotation = rot;
                }

                float distToPoint = Vector3.Distance(transform.position, targetPoint.position);
                if (distToPoint <= patrolPointTolerance)
                {
                    _currentPatrolIndex++;
                    if (_currentPatrolIndex >= patrolPoints.Length)
                        _currentPatrolIndex = 0;
                }
            }
        }
        else
        {
            // Sin patrulla ni player cerca -> quieto
            agent.isStopped = true;
        }
    }

    // ---------------- JUMP ----------------
    void HandleJump(float distToPlayer)
    {
        if (!enableJump || agent == null) return;
        if (!_inCombat) return;
        if (_isJumping) return;
        if (Time.time < _nextJumpTime) return;

        // Ejemplo: solo salta si está a media distancia
        if (distToPlayer > meleeRange && distToPlayer < rangedRange)
        {
            StartCoroutine(JumpRoutine());
            _nextJumpTime = Time.time + jumpCooldown;
        }
    }

    IEnumerator JumpRoutine()
    {
        _isJumping = true;
        agent.isStopped = true;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;
            // Parabola sencilla 0 -> 1 -> 0
            float height = 4f * jumpHeight * t * (1f - t);

            Vector3 pos = startPos;
            pos.y += height;
            transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Asegurar que vuelve al suelo
        Vector3 finalPos = transform.position;
        finalPos.y = startPos.y;
        transform.position = finalPos;

        agent.isStopped = false;
        _isJumping = false;
    }

    // ---------------- ATAQUES ----------------
    void HandleAttacks(float dist)
    {
        if (dist <= meleeRange)
        {
            TryMeleeAttack();
        }
        else if (dist <= rangedRange)
        {
            TryRangedAttack();
        }
    }

    void TryRangedAttack()
    {
        if (Time.time < _nextRangedTime) return;
        if (paintProjectilePrefab == null || firePoint == null) return;

        Vector3 dir = (playerTarget.position + Vector3.up * 1.2f - firePoint.position).normalized;

        Quaternion rot = Quaternion.LookRotation(dir);
        GameObject projGO = Instantiate(paintProjectilePrefab, firePoint.position, rot);

        PaintProjectileBehavior proj = projGO.GetComponent<PaintProjectileBehavior>();
        if (proj != null)
        {
            proj.damage = rangedDamage;
        }

        _nextRangedTime = Time.time + rangedCooldown;
    }

    void TryMeleeAttack()
    {
        if (Time.time < _nextMeleeTime) return;

        Vector3 origin = meleeOrigin != null ? meleeOrigin.position : transform.position;
        float radius = meleeRange;

        Collider[] hits = Physics.OverlapSphere(origin, radius, playerLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            var hp = hit.GetComponent<PlayerLevel>();
            if (hp != null) hp.TakeDamage(meleeDamage);

            Debug.Log($"GraffityCat dio golpe melee al jugador por {meleeDamage}");
            break;
        }

        _nextMeleeTime = Time.time + meleeCooldown;
    }

    // ---------------- MUROS ----------------
    void HandleWalls(float distToPlayer)
    {
        if (!_inCombat) return;
        if (paintWallPrefab == null || wallSpawnPoints == null || wallSpawnPoints.Length == 0)
            return;
        if (Time.time < _nextWallTime) return;
        if (distToPlayer > wallUseRange) return;

        // Spawnea muros en todos los puntos configurados (por ejemplo 2 hijos delante del boss)
        foreach (Transform spawn in wallSpawnPoints)
        {
            if (spawn == null) continue;

            GameObject wallGO = Instantiate(paintWallPrefab, spawn.position, spawn.rotation);
            PaintWall wall = wallGO.GetComponent<PaintWall>();
            if (wall != null)
            {
                _activeWalls.Add(wall);
            }
        }

        _nextWallTime = Time.time + wallCooldown;
    }

    // ---------------- CLONES ----------------
    void HandleClones()
    {
        if (!_inCombat) return;
        if (clonePrefab == null || cloneSpawnPoints == null || cloneSpawnPoints.Length == 0)
            return;

        // limpiar clones muertos
        _aliveClones.RemoveAll(c => c == null);

        if (_aliveClones.Count >= maxClones) return;
        if (Time.time < _nextCloneTime) return;

        // Spawn en un punto aleatorio
        Transform spawn = cloneSpawnPoints[Random.Range(0, cloneSpawnPoints.Length)];
        if (spawn != null)
        {
            GameObject cloneGO = Instantiate(clonePrefab, spawn.position, spawn.rotation);
            GraffityCatCloneEnemy clone = cloneGO.GetComponent<GraffityCatCloneEnemy>();
            if (clone != null)
            {
                // El clon ya tiene lógica para perseguir y atacar al player
                clone.InitClone(playerTarget, this);
                _aliveClones.Add(clone);
            }
        }

        _nextCloneTime = Time.time + cloneCooldown;
    }

    // Llamado por los clones si mueren
    public void NotifyCloneDead(GraffityCatCloneEnemy clone)
    {
        _aliveClones.Remove(clone);
    }

    // ---------------- CLEANUP AL MORIR ----------------
    void OnDestroy()
    {
        // Destruir clones que queden vivos
        foreach (var clone in _aliveClones)
        {
            if (clone != null)
                Destroy(clone.gameObject);
        }
        _aliveClones.Clear();

        // Destruir muros activos
        foreach (var wall in _activeWalls)
        {
            if (wall != null)
                Destroy(wall.gameObject);
        }
        _activeWalls.Clear();
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo melee
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeOrigin ? meleeOrigin.position : transform.position, meleeRange);

        // Gizmo rango ranged
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRange);

        // Gizmos de puntos de muros
        if (wallSpawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var t in wallSpawnPoints)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.position, 0.3f);
            }
        }

        // Gizmos de puntos de clones
        if (cloneSpawnPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var t in cloneSpawnPoints)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.position, 0.3f);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Fusion; // ¡Añadido para multijugador!

public class GraffityCatEnemy : NetworkBehaviour // Cambiado a NetworkBehaviour
{
    public enum BossPhase { Patrol, Combat, Enraged }

    // Sincronizamos la fase para que todos sepan si está enojado
    [Networked] private BossPhase currentPhase { get; set; }

    [SerializeField] private bool enableEnragedPhase = true;
    [SerializeField] private float enragedHpThreshold = 0.4f;
    private bool _enragedApplied = false;

    [Header("Datos base")]
    public EnemyType enemyData;
    public EnemyStats enemyStats;

    [Header("Refs")]
    public NavMeshAgent agent;
    public Transform playerTarget;

    // ---------------- PATRULLA ----------------
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeedMultiplier = 0.6f;
    public float patrolPointTolerance = 0.3f;

    private int _currentPatrolIndex = 0;
    private bool _hasPatrolRoute = false;
    private bool _inCombat = false;

    [Header("Distancia con el jugador")]
    public float desiredCombatDistance = 2f;

    // ---------------- RANGED ----------------
    [Header("Ataque a distancia")]
    public GameObject paintProjectilePrefab;
    public Transform firePoint;
    public float rangedRange = 18f;
    public float rangedCooldown = 2f;
    public int rangedDamage = 20;

    // ---------------- MELEE ----------------
    [Header("Ataque melee fuerte")]
    public float meleeRange = 2.5f;
    public float meleeCooldown = 1.5f;
    public int meleeDamage = 40;
    public Transform meleeOrigin;
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
    private float _jumpTimer = 0f;
    private Vector3 _jumpStartPos;

    // Timers ataques
    private float _nextRangedTime;
    private float _nextMeleeTime;

    private float lastTargetSearchTime;
    [SerializeField] private float targetSearchInterval = 0.5f;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (enemyStats == null) enemyStats = GetComponent<EnemyStats>();
    }

    public override void Spawned()
    {
        // Solo el servidor inicializa la IA y maneja el NavMesh
        if (!HasStateAuthority)
        {
            if (agent != null) agent.enabled = false;
            return;
        }

        currentPhase = BossPhase.Patrol;
        FindClosestPlayer();

        if (enemyData != null && agent != null)
        {
            agent.speed = enemyData.moveSpeed;
            agent.stoppingDistance = desiredCombatDistance;
        }

        _hasPatrolRoute = patrolPoints != null && patrolPoints.Length > 0;
        _currentPatrolIndex = 0;
    }

    // Cambiamos Update por FixedUpdateNetwork
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (Runner.SimulationTime >= lastTargetSearchTime + targetSearchInterval)
        {
            FindClosestPlayer();
            lastTargetSearchTime = Runner.SimulationTime;
        }

        // Si el jugador se desconectó o murió, buscamos otro
        if (playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
        {
            FindClosestPlayer();
            if (playerTarget == null) return; // Si no hay nadie, esperamos
        }

        HandleEnragedPhase();

        // 1. Lógica de Salto (Convertida para funcionar en red sin Corrutinas)
        if (_isJumping)
        {
            _jumpTimer += Runner.DeltaTime;
            float t = _jumpTimer / jumpDuration;

            if (t >= 1f)
            {
                _isJumping = false;
                agent.isStopped = false;
                Vector3 finalPos = transform.position;
                finalPos.y = _jumpStartPos.y;
                transform.position = finalPos;
            }
            else
            {
                float height = 4f * jumpHeight * t * (1f - t);
                Vector3 pos = _jumpStartPos;
                pos.y += height;
                transform.position = pos;
            }
            return; // Mientras salta, no hace nada más
        }

        // 2. Lógica normal
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        bool playerInChaseRange = enemyData != null ? dist <= enemyData.chaseRange : dist <= rangedRange;

        if (playerInChaseRange) _inCombat = true;

        HandleMovement(dist, playerInChaseRange);

        if (_inCombat)
        {
            HandleAttacks(dist);
            HandleWalls(dist);
            HandleClones();
            HandleJump(dist);
        }
    }

    void FindClosestPlayer()
    {
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

        playerTarget = bestTarget;
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

    void HandleMovement(float distToPlayer, bool playerInChaseRange)
    {
        if (agent == null) return;

        if (playerInChaseRange)
        {
            agent.isStopped = false;
            if (enemyData != null) agent.speed = enemyData.moveSpeed;
            agent.stoppingDistance = desiredCombatDistance;
            agent.SetDestination(playerTarget.position);

            Vector3 lookDir = playerTarget.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        else if (_hasPatrolRoute)
        {
            agent.isStopped = false;
            if (enemyData != null) agent.speed = enemyData.moveSpeed * patrolSpeedMultiplier;

            Transform targetPoint = patrolPoints[_currentPatrolIndex];
            if (targetPoint != null)
            {
                agent.stoppingDistance = 0f;
                agent.SetDestination(targetPoint.position);

                Vector3 lookDir = targetPoint.position - transform.position;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(lookDir);

                if (Vector3.Distance(transform.position, targetPoint.position) <= patrolPointTolerance)
                {
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                }
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    void HandleJump(float distToPlayer)
    {
        if (!enableJump || agent == null || !_inCombat || _isJumping) return;
        if (Runner.SimulationTime < _nextJumpTime) return;

        if (distToPlayer > meleeRange && distToPlayer < rangedRange)
        {
            _isJumping = true;
            _jumpTimer = 0f;
            _jumpStartPos = transform.position;
            agent.isStopped = true;

            _nextJumpTime = Runner.SimulationTime + jumpCooldown;
        }
    }

    void HandleAttacks(float dist)
    {
        if (dist <= meleeRange) TryMeleeAttack();
        else if (dist <= rangedRange) TryRangedAttack();
    }

    void TryRangedAttack()
    {
        if (Runner.SimulationTime < _nextRangedTime) return;
        if (paintProjectilePrefab == null || firePoint == null) return;

        Vector3 dir = (playerTarget.position + Vector3.up * 1.2f - firePoint.position).normalized;

        NetworkObject netPrefab = paintProjectilePrefab.GetComponent<NetworkObject>();
        if (netPrefab != null)
        {
            NetworkObject projGO = Runner.Spawn(netPrefab, firePoint.position, Quaternion.LookRotation(dir));
            PaintProjectileBehavior proj = projGO.GetComponent<PaintProjectileBehavior>();
            if (proj != null) proj.damage = rangedDamage;
        }

        _nextRangedTime = Runner.SimulationTime + rangedCooldown;
    }

    void TryMeleeAttack()
    {
        if (Runner.SimulationTime < _nextMeleeTime) return;

        Vector3 origin = meleeOrigin != null ? meleeOrigin.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, meleeRange, playerLayer);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            var hp = hit.GetComponent<PlayerLevel>();
            if (hp != null) hp.TakeDamage(meleeDamage);
            break;
        }

        _nextMeleeTime = Runner.SimulationTime + meleeCooldown;
    }

    void HandleWalls(float distToPlayer)
    {
        if (!_inCombat || paintWallPrefab == null || wallSpawnPoints.Length == 0) return;
        if (Runner.SimulationTime < _nextWallTime || distToPlayer > wallUseRange) return;

        NetworkObject wallNetPrefab = paintWallPrefab.GetComponent<NetworkObject>();
        if (wallNetPrefab != null)
        {
            foreach (Transform spawn in wallSpawnPoints)
            {
                if (spawn == null) continue;
                NetworkObject wallGO = Runner.Spawn(wallNetPrefab, spawn.position, spawn.rotation);
                PaintWall wall = wallGO.GetComponent<PaintWall>();
                if (wall != null) _activeWalls.Add(wall);
            }
        }

        _nextWallTime = Runner.SimulationTime + wallCooldown;
    }

    void HandleClones()
    {
        if (!_inCombat || clonePrefab == null || cloneSpawnPoints.Length == 0) return;

        _aliveClones.RemoveAll(c => c == null);
        if (_aliveClones.Count >= maxClones || Runner.SimulationTime < _nextCloneTime) return;

        Transform spawn = cloneSpawnPoints[Random.Range(0, cloneSpawnPoints.Length)];
        NetworkObject cloneNetPrefab = clonePrefab.GetComponent<NetworkObject>();

        if (spawn != null && cloneNetPrefab != null)
        {
            NetworkObject cloneGO = Runner.Spawn(cloneNetPrefab, spawn.position, spawn.rotation);
            GraffityCatCloneEnemy clone = cloneGO.GetComponent<GraffityCatCloneEnemy>();
            if (clone != null)
            {
                clone.InitClone(playerTarget, this);
                _aliveClones.Add(clone);
            }
        }

        _nextCloneTime = Runner.SimulationTime + cloneCooldown;
    }

    public void NotifyCloneDead(GraffityCatCloneEnemy clone)
    {
        _aliveClones.Remove(clone);
    }

    // ---------------- CLEANUP EN RED ----------------
    // Usamos Despawned en lugar de OnDestroy para manejar la limpieza oficial en el servidor
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (hasState) // Solo el servidor destruye los objetos hijos
        {
            foreach (var clone in _aliveClones)
            {
                if (clone != null && clone.Object != null) runner.Despawn(clone.Object);
            }
            _aliveClones.Clear();

            foreach (var wall in _activeWalls)
            {
                if (wall != null && wall.Object != null) runner.Despawn(wall.Object);
            }
            _activeWalls.Clear();
        }
    }
}

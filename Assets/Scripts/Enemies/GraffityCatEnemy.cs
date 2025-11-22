using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GraffityCatEnemy : MonoBehaviour
{
    [Header("Datos base")]
    public EnemyType enemyData;          // ScriptableObject del boss
    public EnemyStats enemyStats;        // Mismo componente que usan otros enemigos

    [Header("Refs")]
    public NavMeshAgent agent;
    public Transform modelRoot;          // Opcional, para rotar visualmente al gato
    public Transform playerTarget;       // Se autocompleta por tag Player si se deja vacío

    [Header("Ataque a distancia (latas de pintura)")]
    public GameObject paintProjectilePrefab;
    public Transform firePoint;          // Boca de disparo
    public float rangedRange = 18f;
    public float rangedCooldown = 2f;
    public int rangedDamage = 20;

    [Header("Ataque melee fuerte")]
    public float meleeRange = 2.5f;
    public float meleeCooldown = 1.5f;
    public int meleeDamage = 40;
    public Transform meleeOrigin;        // centro del golpe (puede ser el mismo que firePoint)
    public LayerMask playerLayer;

    [Header("Muros de pintura")]
    public GameObject paintWallPrefab;
    public Transform[] wallSpawnPoints;  // posiciones prefijadas en la arena
    public float wallCooldown = 8f;

    [Header("Clones")]
    public GameObject clonePrefab;
    public Transform[] cloneSpawnPoints;
    public int maxClones = 2;
    public float cloneCooldown = 12f;

    private float nextRangedTime;
    private float nextMeleeTime;
    private float nextWallTime;
    private float nextCloneTime;

    private readonly List<GraffityCatCloneEnemy> _aliveClones = new List<GraffityCatCloneEnemy>();

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
            agent.speed = enemyData.moveSpeed;     // boss más rápido
            agent.stoppingDistance = meleeRange * 0.8f;
        }
    }

    void Update()
    {
        if (playerTarget == null || enemyStats == null) return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);

        HandleMovement(dist);
        HandleAttacks(dist);
        HandleWalls();
        HandleClones();
    }

    void HandleMovement(float dist)
    {
        if (enemyData == null || agent == null) return;

        if (dist <= enemyData.chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);

            // rotar modelo hacia el player (solo en XZ)
            Vector3 lookDir = playerTarget.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir);
                if (modelRoot != null) modelRoot.rotation = rot;
                else transform.rotation = rot;
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    void HandleAttacks(float dist)
    {
        // Prioridad: si está muy cerca -> melee, si está a media distancia -> ranged
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
        if (Time.time < nextRangedTime) return;
        if (paintProjectilePrefab == null || firePoint == null) return;

        // Apuntar al player
        Vector3 dir = (playerTarget.position - firePoint.position).normalized;
        dir.y = 0;  // proyectil horizontal (podés quitar esto si querés arco)

        Quaternion rot = Quaternion.LookRotation(dir);
        GameObject projGO = Instantiate(paintProjectilePrefab, firePoint.position, rot);

        // Setear daño si el script del proyectil lo soporta
        PaintProjectileBehavior proj = projGO.GetComponent<PaintProjectileBehavior>();
        if (proj != null)
        {
            proj.damage = rangedDamage;
        }

        nextRangedTime = Time.time + rangedCooldown;
    }

    void TryMeleeAttack()
    {
        if (Time.time < nextMeleeTime) return;

        Vector3 origin = meleeOrigin != null ? meleeOrigin.position : transform.position;
        float radius = meleeRange;

        Collider[] hits = Physics.OverlapSphere(origin, radius, playerLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            //Cambiá estos nombres por tu script real de vida del jugador
            var hp1 = hit.GetComponent<PlayerLevel>();
            if (hp1 != null) hp1.TakeDamage(meleeDamage);

            var hp2 = hit.GetComponent<PlayerLevel>();
            if (hp2 != null) hp2.TakeDamage(meleeDamage);

            Debug.Log($"GraffityCat dio golpe melee al jugador por {meleeDamage}");
            break;
        }

        nextMeleeTime = Time.time + meleeCooldown;
    }

    void HandleWalls()
    {
        if (paintWallPrefab == null || wallSpawnPoints == null || wallSpawnPoints.Length == 0)
            return;

        if (Time.time < nextWallTime) return;

        // Boss va "grafiteando" un muro en uno de los puntos al azar
        Transform spawn = wallSpawnPoints[Random.Range(0, wallSpawnPoints.Length)];
        Instantiate(paintWallPrefab, spawn.position, spawn.rotation);

        nextWallTime = Time.time + wallCooldown;
    }

    void HandleClones()
    {
        if (clonePrefab == null || cloneSpawnPoints == null || cloneSpawnPoints.Length == 0)
            return;

        // limpiar clones muertos
        _aliveClones.RemoveAll(c => c == null);

        if (_aliveClones.Count >= maxClones) return;
        if (Time.time < nextCloneTime) return;

        Transform spawn = cloneSpawnPoints[Random.Range(0, cloneSpawnPoints.Length)];
        GameObject cloneGO = Instantiate(clonePrefab, spawn.position, spawn.rotation);

        GraffityCatCloneEnemy clone = cloneGO.GetComponent<GraffityCatCloneEnemy>();
        if (clone != null)
        {
            clone.InitClone(playerTarget, this);
            _aliveClones.Add(clone);
        }

        nextCloneTime = Time.time + cloneCooldown;
    }

    // Llamado por los clones si mueren
    public void NotifyCloneDead(GraffityCatCloneEnemy clone)
    {
        _aliveClones.Remove(clone);
    }

    void OnDrawGizmosSelected()
    {
        // Gizmos de ayuda
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleeOrigin ? meleeOrigin.position : transform.position, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}

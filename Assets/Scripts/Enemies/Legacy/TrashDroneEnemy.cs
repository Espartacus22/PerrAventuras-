using UnityEngine;


public class TrashDroneEnemy : MonoBehaviour
{
    [Header("Data (opcional)")]
    public EnemyType enemyData;

    [Header("Patrulla (opcional)")]
    public Transform[] patrolPoints;
    public float moveSpeed = 4f;
    public float pointTolerance = 0.5f;
    public float hoverHeight = 3f;

    [Header("Detección / Combate")]
    public float detectRange = 20f;
    public float desiredCombatDistance = 3f;
    public float combatMoveSpeed = 4.5f;
    public float distanceDeadZone = 0.5f;

    [Header("Disparo")]
    public Transform[] firePoints;
    public GameObject trashProjectilePrefab;
    public float shootRange = 15f;
    public float shootCooldown = 1.2f;
    public float projectileDamage = 10f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float obstacleCheckDistance = 1f;
    public float obstacleRayHeight = 0.2f;

    private Transform targetPlayer;
    private int currentPoint;
    private float nextShootTime;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            targetPlayer = p.transform;

        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            detectRange = enemyData.chaseRange;
            projectileDamage = enemyData.meleeDamage;
        }

        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, hoverHeight, pos.z);

        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            Vector3 start = patrolPoints[0].position;
            transform.position = new Vector3(start.x, hoverHeight, start.z);
        }
    }

    private void Update()
    {
        if (targetPlayer == null) return;

        float distToPlayerXZ = GetFlatDistance(transform.position, targetPlayer.position);
        bool playerInDetectRange = distToPlayerXZ <= detectRange;

        if (!playerInDetectRange && patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            HandlePatrol();
        }
        else if (playerInDetectRange)
        {
            HandleCombatMovement();
            HandleShooting(distToPlayerXZ);
        }
    }

    private float GetFlatDistance(Vector3 a, Vector3 b)
    {
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        return Vector2.Distance(a2, b2);
    }

    private bool IsPathBlocked(Vector3 moveDir)
    {
        Vector3 origin = transform.position + Vector3.up * obstacleRayHeight;
        return Physics.Raycast(origin, moveDir.normalized, obstacleCheckDistance, obstacleMask);
    }

    private void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform point = patrolPoints[currentPoint];
        if (point == null) return;

        Vector3 targetPos = new Vector3(point.position.x, hoverHeight, point.position.z);
        Vector3 moveDir = (targetPos - transform.position);
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(moveDir.normalized);

        if (!IsPathBlocked(moveDir))
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
        }

        float dist = GetFlatDistance(transform.position, targetPos);
        if (dist <= pointTolerance)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
    }

    private void HandleCombatMovement()
    {
        Vector3 playerFlat = new Vector3(targetPlayer.position.x, hoverHeight, targetPlayer.position.z);
        Vector3 droneFlat = new Vector3(transform.position.x, hoverHeight, transform.position.z);

        Vector3 toPlayerFlat = playerFlat - droneFlat;
        float distance = toPlayerFlat.magnitude;

        Vector3 lookDir = new Vector3(toPlayerFlat.x, 0f, toPlayerFlat.z);
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir.normalized);

        if (distance > desiredCombatDistance + distanceDeadZone)
        {
            Vector3 moveDir = lookDir.normalized;
            float moveStep = combatMoveSpeed * Time.deltaTime;

            if (!IsPathBlocked(moveDir))
            {
                Vector3 newPos = droneFlat + moveDir * moveStep;
                transform.position = new Vector3(newPos.x, hoverHeight, newPos.z);
            }
        }
        else if (distance < desiredCombatDistance - distanceDeadZone)
        {
            Vector3 moveDir = -lookDir.normalized;
            float moveStep = combatMoveSpeed * Time.deltaTime;

            if (!IsPathBlocked(moveDir))
            {
                Vector3 newPos = droneFlat + moveDir * moveStep;
                transform.position = new Vector3(newPos.x, hoverHeight, newPos.z);
            }
        }
        else
        {
            transform.position = new Vector3(transform.position.x, hoverHeight, transform.position.z);
        }
    }

    private void HandleShooting(float distToPlayerXZ)
    {
        if (trashProjectilePrefab == null || firePoints == null || firePoints.Length == 0)
            return;

        if (distToPlayerXZ > shootRange) return;
        if (Time.time < nextShootTime) return;

        foreach (Transform fp in firePoints)
        {
            if (fp == null) continue;

            Vector3 toTarget = (targetPlayer.position + Vector3.up * 1.2f) - fp.position;
            if (toTarget.sqrMagnitude < 0.001f) continue;

            Quaternion rot = Quaternion.LookRotation(toTarget.normalized);
            GameObject projectile = Instantiate(trashProjectilePrefab, fp.position, rot);

            EnemyProjectileBehavior proj = projectile.GetComponent<EnemyProjectileBehavior>();
            if (proj != null)
            {
                proj.SetDamage(projectileDamage);
                proj.SetRange(shootRange);
            }
        }

        nextShootTime = Time.time + shootCooldown;
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        Gizmos.color = Color.cyan;
        Vector3 flatCenter = new Vector3(transform.position.x, hoverHeight, transform.position.z);
        Gizmos.DrawWireSphere(flatCenter, desiredCombatDistance);

        Gizmos.color = Color.magenta;
        Vector3 origin = transform.position + Vector3.up * obstacleRayHeight;
        Gizmos.DrawRay(origin, transform.forward * obstacleCheckDistance);
    }
}

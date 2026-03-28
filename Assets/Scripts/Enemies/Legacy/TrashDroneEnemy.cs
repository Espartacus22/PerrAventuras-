using UnityEngine;


public class TrashDroneEnemy : MonoBehaviour
{
    [Header("Patrulla (opcional)")]
    public Transform[] patrolPoints;
    public float moveSpeed = 4f;
    public float pointTolerance = 0.5f;
    public float hoverHeight = 3f;

    [Header("Deteccion / Combate")]
    public float detectRange = 20f;
    public float desiredCombatDistance = 1.2f;
    public float combatMoveSpeed = 4.5f;
    public float distanceDeadZone = 0.25f;

    [Header("Disparo")]
    public Transform[] firePoints;
    public GameObject trashProjectilePrefab;
    public float shootRange = 15f;
    public float shootCooldown = 1.2f;

    Transform targetPlayer;
    int currentPoint;
    float nextShootTime;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            targetPlayer = p.transform;

        // Mantener altura fija
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, hoverHeight, pos.z);

        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            Vector3 start = patrolPoints[0].position;
            transform.position = new Vector3(start.x, hoverHeight, start.z);
        }
    }

    void Update()
    {
        if (targetPlayer == null) return;

        float distToPlayerXZ = GetFlatDistance(transform.position, targetPlayer.position);

        bool playerInDetectRange = distToPlayerXZ <= detectRange;

        if (!playerInDetectRange &&
            patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            HandlePatrol();
        }
        else
        {
            HandleCombatMovement(distToPlayerXZ);
        }

        HandleShooting(distToPlayerXZ);
    }

    float GetFlatDistance(Vector3 a, Vector3 b)
    {
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        return Vector2.Distance(a2, b2);
    }

    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform point = patrolPoints[currentPoint];
        if (point == null) return;

        Vector3 targetPos = new Vector3(point.position.x, hoverHeight, point.position.z);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        float dist = GetFlatDistance(transform.position, targetPos);
        if (dist <= pointTolerance)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
    }

    void HandleCombatMovement(float distToPlayerXZ)
    {
        Vector3 playerFlat = new Vector3(targetPlayer.position.x, hoverHeight, targetPlayer.position.z);
        Vector3 droneFlat = new Vector3(transform.position.x, hoverHeight, transform.position.z);

        Vector3 toPlayerFlat = playerFlat - droneFlat;
        float distance = toPlayerFlat.magnitude;

        // Rotar hacia el jugador en XZ
        Vector3 lookDir = new Vector3(toPlayerFlat.x, 0f, toPlayerFlat.z);
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (distance > desiredCombatDistance + distanceDeadZone)
        {
            // acercarse
            Vector3 moveDir = (playerFlat - droneFlat).normalized;
            float moveStep = combatMoveSpeed * Time.deltaTime;

            if (!Physics.Raycast(transform.position, moveDir, moveStep + 0.5f))
            {
                Vector3 newPos = droneFlat + moveDir * moveStep;
                transform.position = new Vector3(newPos.x, hoverHeight, newPos.z);
            }
        }
        else if (distance < desiredCombatDistance - distanceDeadZone)
        {
            // alejarse un poco
            Vector3 away = -toPlayerFlat.normalized;
            Vector3 moveDir = away.normalized;
            float moveStep = combatMoveSpeed * Time.deltaTime;

            // Raycast para detectar pared
            if (!Physics.Raycast(transform.position, moveDir, moveStep + 0.5f))
            {
                Vector3 newPos = droneFlat + moveDir * moveStep;
                transform.position = new Vector3(newPos.x, hoverHeight, newPos.z);
            }
        }
        else
        {
            // dentro de la distancia ideal, solo corrige altura
            transform.position = new Vector3(transform.position.x, hoverHeight, transform.position.z);
        }
    }

    void HandleShooting(float distToPlayerXZ)
    {
        if (trashProjectilePrefab == null || firePoints == null || firePoints.Length == 0)
            return;

        if (distToPlayerXZ > shootRange) return;
        if (Time.time < nextShootTime) return;

        foreach (var fp in firePoints)
        {
            if (fp == null) continue;

            Vector3 toTarget = (targetPlayer.position + Vector3.up * 1.2f) - fp.position;
            if (toTarget.sqrMagnitude < 0.001f) continue;

            Quaternion rot = Quaternion.LookRotation(toTarget.normalized);
            Instantiate(trashProjectilePrefab, fp.position, rot);
        }

        nextShootTime = Time.time + shootCooldown;
    }
}

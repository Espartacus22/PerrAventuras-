using UnityEngine;


public class TrashDroneEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float moveSpeed = 4f;
    public float pointTolerance = 0.5f;
    public float hoverHeight = 3f;

    [Header("Disparo")]
    public Transform[] firePoints;          // dos extremos del cilindro
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

        // Aseguramos que el dron arranque flotando
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, hoverHeight, pos.z);
    }

    void Update()
    {
        HandlePatrol();
        HandleShooting();
    }

    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform point = patrolPoints[currentPoint];
        Vector3 targetPos = new Vector3(point.position.x, hoverHeight, point.position.z);

        transform.position = Vector3.MoveTowards(transform.position,
                                                 targetPos,
                                                 moveSpeed * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist <= pointTolerance)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }
    }

    void HandleShooting()
    {
        if (targetPlayer == null) return;

        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        if (dist > shootRange) return;
        if (Time.time < nextShootTime) return;

        // Rotar el dron mirando al player (solo en XZ)
        Vector3 dir = targetPlayer.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        // Disparar desde todos los firePoints
        if (trashProjectilePrefab != null && firePoints != null)
        {
            foreach (var fp in firePoints)
            {
                if (fp == null) continue;
                Instantiate(trashProjectilePrefab, fp.position, fp.rotation);
            }
        }

        nextShootTime = Time.time + shootCooldown;
    }
}

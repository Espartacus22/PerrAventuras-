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

        // Si hay patrolPoints, arrancar desde el primero
        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            Vector3 start = patrolPoints[0].position;
            transform.position = new Vector3(start.x, hoverHeight, start.z);
        }
    }

    void Update()
    {
        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            HandlePatrol();   // se mueve de punto a punto
                              // si no hay puntos, NO llamamos a HandlePatrol y el dron se queda quieto flotando

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

        // 1) Rotar el dron en horizontal hacia el player (opcional, para que "mire" al player)
        Vector3 flatDir = targetPlayer.position - transform.position;
        flatDir.y = 0;
        if (flatDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flatDir);

        // 2) Disparar desde TODOS los firePoints apuntando EXACTO al player
        if (trashProjectilePrefab != null && firePoints != null)
        {
            foreach (var fp in firePoints)
            {
                if (fp == null) continue;

                // dirección desde este firePoint hacia el jugador (un poco a la altura del pecho)
                Vector3 toTarget = (targetPlayer.position + Vector3.up * 1.2f) - fp.position;
                if (toTarget.sqrMagnitude < 0.001f) continue;

                Quaternion rot = Quaternion.LookRotation(toTarget.normalized);

                Instantiate(trashProjectilePrefab, fp.position, rot);
            }
        }

        nextShootTime = Time.time + shootCooldown;
    }
}

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SkaterCatEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    private int idx = 0;

    [Header("Detección")]
    public Transform player;
    public float outerDetectionRange = 16f;     // rango para empezar a perseguir/disparar
    public float innerDetectionRange = 6f;      // rango que activa countdown (ventana)
    public float initialDelayInInner = 2.5f;    // segundos de ventaja antes de disparar al entrar al inner range
    public float preferredDistance = 10f;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float shootInterval = 2f;

    [Header("HP / Furia")]
    public int maxHP = 1;
    private int hp;
    private bool inFury = false;
    public float furyTimeout = 3f;

    // internals
    private CharacterController controller;
    private float lastShotTime = 0f;
    private bool isCountingDown = false;
    private bool playerInOuter = false;
    private Vector3 gravityVel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        hp = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        playerInOuter = dist <= outerDetectionRange;

        if (dist <= innerDetectionRange)
        {
            if (!isCountingDown)
            {
                StartCoroutine(InnerRangeCountdown());
            }
            // while counting down, don't immediately fire - gives player time to act
            MaintainDistance(dist);
        }
        else if (playerInOuter)
        {
            // comportamiento normal: mantener distancia y disparar según intervalos
            MaintainDistance(dist);
            float currentInterval = inFury ? (shootInterval * 0.5f) : shootInterval;
            if (Time.time - lastShotTime > currentInterval)
            {
                Shoot();
                lastShotTime = Time.time;
            }
        }
        else
        {
            Patrol();
        }

        ApplyGravity();
    }

    private IEnumerator InnerRangeCountdown()
    {
        isCountingDown = true;
        float t0 = Time.time;
        // espera inicial — da ventaja para rematar
        while (Time.time - t0 < initialDelayInInner)
        {
            // si el enemigo muere durante este tiempo, lo paramos
            if (hp <= 0) break;
            yield return null;
        }

        // si sigue vivo: empieza a disparar (furia parcial o normal)
        isCountingDown = false;
        lastShotTime = Time.time; // disparará según shootInterval
    }

    void MaintainDistance(float dist)
    {
        // mover para mantenerse sobre preferredDistance
        if (dist < preferredDistance - 0.5f)
            MoveAwayFrom(player.position);
        else if (dist > preferredDistance + 0.5f)
            MoveTowards(player.position);
        else
            FaceTarget(player.position);
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Transform target = patrolPoints[idx];
        Vector3 dir = (target.position - transform.position);
        dir.y = 0;
        if (dir.magnitude < 0.6f)
        {
            idx = (idx + 1) % patrolPoints.Length;
            return;
        }
        controller.Move(dir.normalized * patrolSpeed * Time.deltaTime);
        FaceTarget(target.position);
    }

    void MoveTowards(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position);
        dir.y = 0;
        controller.Move(dir.normalized * patrolSpeed * Time.deltaTime);
        FaceTarget(pos);
    }

    void MoveAwayFrom(Vector3 pos)
    {
        Vector3 dir = (transform.position - pos);
        dir.y = 0;
        controller.Move(dir.normalized * patrolSpeed * Time.deltaTime);
        FaceTarget(pos);
    }

    void FaceTarget(Vector3 pos)
    {
        Vector3 dir = pos - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.15f);
    }

    void Shoot()
    {
        if (projectilePrefab == null || projectileSpawn == null || player == null) return;
        GameObject p = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        ProjectileLocal proj = p.GetComponent<ProjectileLocal>();
        if (proj != null)
            proj.SetDirection((player.position - projectileSpawn.position).normalized);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && gravityVel.y < 0) gravityVel.y = -2f;
        gravityVel.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(gravityVel * Time.deltaTime);
    }

    public void ReceiveDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
        else StartCoroutine(CheckFuryTimer());
    }

    IEnumerator CheckFuryTimer()
    {
        float t0 = Time.time;
        while (Time.time - t0 < furyTimeout)
        {
            if (hp <= 0) yield break;
            yield return null;
        }
        inFury = true;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}

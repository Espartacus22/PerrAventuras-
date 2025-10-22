using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Skater_NavMesh : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    private int currentPoint = 0;
    public float patrolSpeed = 3f;

    [Header("Detección")]
    public Transform player;
    public float outerDetectionRange = 25f;
    public float innerDetectionRange = 8f;
    public float initialDelayInInner = 2.5f;
    public float preferredDistance = 10f;

    [Header("Ataques")]
    public GameObject projectilePrefab;   // Proyectil tipo "sniper"
    public GameObject heavyProjectilePrefab; // Proyectil corto y fuerte
    public Transform projectileSpawn;
    public float shootInterval = 1.5f;

    [Header("HP / Furia")]
    public int maxHP = 3;
    public float furyTimeout = 3f;

    private NavMeshAgent agent;
    private bool canShoot = true;
    private bool playerDetected;
    private bool playerInCloseRange;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.autoBraking = false;
        agent.autoTraverseOffMeshLink = true;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    private void Update()
    {
        if (player == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= outerDetectionRange)
        {
            playerDetected = true;

            if (distance <= innerDetectionRange)
            {
                if (!playerInCloseRange)
                {
                    playerInCloseRange = true;
                    StartCoroutine(HandleInnerRangeAttack());
                }
            }
            else
            {
                playerInCloseRange = false;
                HandleShooting(projectilePrefab, false);
                LookAtTarget(player);
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            playerDetected = false;
            Patrol();
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    private void HandleShooting(GameObject projectile, bool isHeavy)
    {
        if (canShoot && projectile != null && projectileSpawn != null)
        {
            canShoot = false;

            GameObject newProj = Instantiate(projectile, projectileSpawn.position, Quaternion.identity);

            // Apuntar hacia el jugador
            Vector3 direction = (player.position - projectileSpawn.position).normalized;
            newProj.transform.forward = direction;

            // Asignar tipo de objetivo (Player)
            Projectile proj = newProj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetTargetTag("Player");
                if (isHeavy)
                {
                    proj.speed *= 1.8f;
                    proj.damage *= 2;
                }
            }

            StartCoroutine(ResetShootCooldown());
        }
    }

    private IEnumerator ResetShootCooldown()
    {
        yield return new WaitForSeconds(shootInterval);
        canShoot = true;
    }

    private IEnumerator HandleInnerRangeAttack()
    {
        Debug.Log("SkaterCat detectó jugador cerca... preparando ataque fuerte.");
        yield return new WaitForSeconds(initialDelayInInner);
        HandleShooting(heavyProjectilePrefab != null ? heavyProjectilePrefab : projectilePrefab, true);
    }

    private void LookAtTarget(Transform target)
    {
        Vector3 lookDir = (target.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
    }
}

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SkaterCatEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    private int currentPoint = 0;

    [Header("Detección del jugador")]
    public Transform player;
    public float outerDetectionRange = 18f;
    public float innerDetectionRange = 6f;
    public float initialDelayInInner = 2.5f;
    public float preferredDistance = 10f;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float shootInterval = 1f;
    private bool canShoot = true;

    [Header("HP / Furia")]
    public int maxHP = 1;
    public float furyTimeout = 3f;
    private int currentHP;

    private CharacterController controller;
    private Vector3 moveDir;

    private bool playerDetected;
    private bool playerInInnerRange;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHP = maxHP;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        //Modo ataque
        if (distance < innerDetectionRange)
        {
            playerInInnerRange = true;
            StartCoroutine(InnerRangeBehavior());
        }
        else if (distance < outerDetectionRange)
        {
            playerDetected = true;
            HandleShooting();
        }
        else
        {
            playerDetected = false;
            Patrol();
        }
    }

    #region MÉTODOS DE PATRULLA

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Vector3 target = patrolPoints[currentPoint].position;
        Vector3 direction = (target - transform.position).normalized;

        // Movimiento con CharacterController
        controller.Move(direction * patrolSpeed * Time.deltaTime);

        // Rotar hacia el punto
        if (direction.magnitude > 0.1f)
            transform.rotation = Quaternion.LookRotation(direction);

        // Pasar al siguiente punto
        if (Vector3.Distance(transform.position, target) < 0.5f)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }
    #endregion

    #region MÉTODOS DE ATAQUE
    private void HandleShooting()
    {
        if (projectilePrefab == null || projectileSpawn == null) return;

        if (canShoot)
        {
            canShoot = false;
            Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
            StartCoroutine(ResetShootCooldown());
        }
    }
    
    IEnumerator ResetShootCooldown()
    {
        yield return new WaitForSeconds(shootInterval);
        canShoot = true;
    }

    IEnumerator InnerRangeBehavior()
    {
        if (playerInInnerRange)
        {
            yield return new WaitForSeconds(initialDelayInInner);
            HandleShooting();
        }
        playerInInnerRange = false;
    }
    #endregion

    #region MÉTODOS DE DAÑO
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}

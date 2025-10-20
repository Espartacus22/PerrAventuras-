using UnityEngine;

public class EnemyIA_Follow : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    private int currentPointIndex = 0;

    [Header("Persecución")]
    public float detectionRange = 15f;
    public float chaseSpeed = 6f;
    public float attackRange = 2f;

    [Header("Referencias")]
    public Transform player;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        ApplyGravity();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        controller.Move(direction * patrolSpeed * Time.deltaTime);

        // Rotar hacia el siguiente punto
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);

        // Avanzar al siguiente waypoint si estamos cerca
        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        controller.Move(direction * chaseSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
    }

    private void AttackPlayer()
    {
        Debug.Log("Atacando al jugador...");
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
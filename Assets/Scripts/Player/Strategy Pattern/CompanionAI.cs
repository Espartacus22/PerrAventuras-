using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FollowPlayer))]
[RequireComponent(typeof(ProjectileLocal))]
public class CompanionAI : MonoBehaviour
{
    public Transform target;
    public float followDistance = 2.5f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public float runSpeed = 6f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isRunning;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!target) return;

        float distance = Vector3.Distance(transform.position, target.position);

        bool targetIsRunning = Input.GetKey(KeyCode.LeftShift); // si PJ1 corre → él también
        agent.speed = targetIsRunning ? runSpeed : moveSpeed;
        isRunning = targetIsRunning;

        if (distance > followDistance)
        {
            agent.SetDestination(target.position);
            animator.SetBool("isMoving", true);
        }
        else
        {
            agent.ResetPath();
            animator.SetBool("isMoving", false);
        }

        // Si hay enemigo cerca → atacar
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var h in hits)
        {
            if (h.CompareTag("Enemy"))
            {
                Attack(h.transform);
            }
        }
    }

    private void Attack(Transform enemy)
    {
        transform.LookAt(enemy);
        Debug.Log("Companion ataca al enemigo!");
        // TODO: envestida real + daño
    }
}

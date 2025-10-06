using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public Transform target; // El jugador
    public float chaseRange = 3f; // distancia máxima para perseguir

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= chaseRange)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            agent.ResetPath(); // detiene el movimiento
        }
    }
}

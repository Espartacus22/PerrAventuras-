using UnityEngine;
using UnityEngine.AI;

public class ChaseTarget : MonoBehaviour
{
    public Transform target; // The target to chase
    private NavMeshAgent agent; // Reference to the NavMeshAgent component

    void Start()
    {
        // Get the NavMeshAgent component attached to this GameObject
        agent = GetComponent<NavMeshAgent>();

        // Ensure the agent is set up properly
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name);
        }
    }

    void Update()
    {
        // Check if the target is assigned
        if (target != null)
        {
            // Set the destination to the target's position
            agent.SetDestination(target.position);
        }
    }
}
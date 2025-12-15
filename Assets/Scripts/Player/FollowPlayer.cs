using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FollowPlayer : MonoBehaviour
{
    public Transform targetToFollow;
    public float followDistance = 2f;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!enabled) return;
        if (agent == null || !agent.enabled) return;
        if (targetToFollow == null) return;

        float distance = Vector3.Distance(transform.position, targetToFollow.position);

        if (distance > followDistance)
            agent.SetDestination(targetToFollow.position);
        else
            agent.ResetPath();
    }
}

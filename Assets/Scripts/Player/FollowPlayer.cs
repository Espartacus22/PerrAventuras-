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

    void OnEnable()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.isStopped = false;
    }

    void Update()
    {
        if (agent == null || !agent.enabled) return;
        if (targetToFollow == null) return;

        float distance = Vector3.Distance(transform.position, targetToFollow.position);

        if (distance > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(targetToFollow.position);
        }
        else
        {
            agent.ResetPath();
        }

    }
}

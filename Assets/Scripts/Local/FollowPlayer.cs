using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour
{
    public Transform targetToFollow;
    public float followDistance = 2f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (targetToFollow == null) return;

        float distance = Vector3.Distance(transform.position, targetToFollow.position);

        if (distance > followDistance)
        {
            agent.SetDestination(targetToFollow.position);
        }
        else
        {
            agent.ResetPath(); // se frena
        }
    }
}

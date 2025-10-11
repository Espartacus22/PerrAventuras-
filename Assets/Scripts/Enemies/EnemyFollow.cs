using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public EnemyType enemyData;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (enemyData != null)
            agent.speed = enemyData.moveSpeed;
    }

    void Update()
    {
        if (target == null || enemyData == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= enemyData.chaseRange)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            agent.ResetPath();
        }
    }
}
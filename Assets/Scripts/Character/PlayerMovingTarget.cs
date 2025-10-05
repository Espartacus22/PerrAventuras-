using UnityEngine;
using UnityEngine.AI;

public class PlayerMovingTarget : MonoBehaviour
{
    private NavMeshAgent agent; // Reference to the NavMeshAgent component
    public float moveSpeed = 5f; // Player movement speed
    public float rotationSpeed = 720f; // Rotation speed in degrees per second

    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name);
        }

        // Configure NavMeshAgent for manual control
        agent.updateRotation = false; // We'll handle rotation manually
        agent.speed = moveSpeed;
    }

    void Update()
    {
        // Get player input
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow
        float moveZ = Input.GetAxisRaw("Vertical"); // W/S or Up/Down Arrow

        // Calculate movement direction
        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        if (movement.magnitude > 0)
        {
            // Calculate the desired position
            Vector3 desiredPosition = transform.position + movement * moveSpeed * Time.deltaTime;

            // Ensure the position is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            // Rotate the player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}

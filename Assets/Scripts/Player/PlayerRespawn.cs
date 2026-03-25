using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Void / caída")]
    [SerializeField] private float voidY = -10f;

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;

    private Rigidbody rb;
    private PlayerLevel playerLevel;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerLevel = GetComponent<PlayerLevel>();
    }

    private void Start()
    {
        lastCheckpointPosition = transform.position;
        lastCheckpointRotation = transform.rotation;
    }

    private void Update()
    {
        if (transform.position.y < voidY)
        {
            Respawn();
        }
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
        Debug.Log("Checkpoint guardado");
    }

    public void Respawn()
    {
        transform.position = lastCheckpointPosition;
        transform.rotation = lastCheckpointRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (playerLevel != null)
        {
            playerLevel.RestoreFullState();
        }

        Debug.Log("Respawn ejecutado");
    }
}

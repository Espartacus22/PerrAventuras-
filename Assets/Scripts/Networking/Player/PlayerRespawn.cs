using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Punto inicial")]
    public Transform defaultSpawnPoint;

    [Header("Checkpoints")]
    public Checkpoint currentCheckpoint;

    [Header("Caída del nivel")]
    public float fallThresholdY = -20f;

    private PlayerLevel playerLevel;
    private Rigidbody rb;
    private PlayerMovement movement;

    void Awake()
    {
        playerLevel = GetComponent<PlayerLevel>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        if (defaultSpawnPoint == null)
        {
            GameObject go = new GameObject("DefaultSpawnPoint");
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
            defaultSpawnPoint = go.transform;
        }

        if (playerLevel != null)
        {
            playerLevel.respawn = this;
        }
    }

    void Update()
    {
        if (transform.position.y < fallThresholdY)
        {
            Respawn();
        }
    }

    public void OnPlayerDeath()
    {
        Respawn();
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        currentCheckpoint = checkpoint;
        Debug.Log($"Checkpoint activado: {checkpoint.name}");
    }

    public void Respawn()
    {
        Vector3 respawnPos = defaultSpawnPoint.position;
        Quaternion respawnRot = defaultSpawnPoint.rotation;

        if (currentCheckpoint != null)
        {
            respawnPos = currentCheckpoint.GetSpawnPosition();
            respawnRot = currentCheckpoint.transform.rotation;
        }

        if (movement != null)
            movement.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = respawnPos;
        transform.rotation = respawnRot;

        if (playerLevel != null)
            playerLevel.RestoreFullState();

        if (movement != null)
            movement.enabled = true;
    }
}

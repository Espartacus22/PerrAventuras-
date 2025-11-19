using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Punto inicial")]
    public Transform defaultSpawnPoint;      // si lo dej�s vac�o, usa la posici�n inicial actual

    [Header("Checkpoints")]
    public Checkpoint currentCheckpoint;

    [Header("Ca�da del nivel")]
    public float fallThresholdY = -20f;      // si el player baja de esta Y, respawnea

    PlayerLevel playerLevel;
    Rigidbody rb;
    PlayerMovement movement;  // tu script de movimiento, cambialo por el nombre real

    void Awake()
    {
        playerLevel = GetComponent<PlayerLevel>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        // Si no se asigna spawnPoint, usamos la posicion donde empezo
        if (defaultSpawnPoint == null)
        {
            GameObject go = new GameObject("DefaultSpawnPoint");
            go.transform.position = transform.position;
            defaultSpawnPoint = go.transform;
        }

        if (playerLevel != null)
        {
            playerLevel.respawn = this;   // conectar desde c�digo por si te olvid�s en el inspector
        }
    }

    void Update()
    {
        // Caida fuera del escenario
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

    void Respawn()
    {
        // Determinar posicion de respawn
        Vector3 respawnPos = defaultSpawnPoint.position;
        Quaternion respawnRot = defaultSpawnPoint.rotation;

        if (currentCheckpoint != null)
        {
            respawnPos = currentCheckpoint.GetSpawnPosition();
            respawnRot = currentCheckpoint.transform.rotation;
        }

        // Desactivar movimiento mientras teletransportamos
        if (movement != null)
            movement.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = respawnPos;
        transform.rotation = respawnRot;

        if (movement != null)
            movement.enabled = true;

        // Recuperar vida
        if (playerLevel != null)
            playerLevel.GetMaxHP();
    }
}

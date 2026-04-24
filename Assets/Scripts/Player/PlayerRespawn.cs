using UnityEngine;
using Fusion;

public class PlayerRespawn : NetworkBehaviour
{
    [Header("Void / caída")]
    [SerializeField] private float voidY = -10f;

    // --- VARIABLES DE RED ---
    [Networked] private Vector3 lastCheckpointPosition { get; set; }
    [Networked] private Quaternion lastCheckpointRotation { get; set; }

    private NetworkCharacterController ncc;
    private PlayerLevel playerLevel;

    private void Awake()
    {
        ncc = GetComponent<NetworkCharacterController>();
        playerLevel = GetComponent<PlayerLevel>();
    }

    public override void Spawned()
    {
        // Al nacer, el servidor guarda el punto inicial como el primer checkpoint
        if (HasStateAuthority)
        {
            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = transform.rotation;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return; // Solo el servidor controla las caídas al vacío

        if (transform.position.y < voidY)
        {
            Respawn();
        }
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        // Solo el servidor tiene derecho a actualizar la bandera del checkpoint
        if (HasStateAuthority)
        {
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
            Debug.Log("[CHECKPOINT] Posición guardada oficialmente en el servidor.");
        }
    }

    public void Respawn()
    {
        if (HasStateAuthority)
        {
            // 1. Teletransportar usando el NetworkCharacterController
            ncc.Teleport(lastCheckpointPosition, lastCheckpointRotation);

            // 2. Frenar el impulso para que el personaje no siga "cayendo"
            ncc.Velocity = Vector3.zero;

            // 3. Restaurar vida
            if (playerLevel != null)
            {
                playerLevel.RestoreFullState();
            }

            Debug.Log("[RESPAWN] ¡Teletransportación ejecutada por red!");
        }
    }
}
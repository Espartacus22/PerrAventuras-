using UnityEngine;

public class TrainingEnd : MonoBehaviour
{
    public NPCs npc;
    public PlayerMovement playerMovement;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Pista completada!");

        // Recompensa
        if (playerMovement != null)
            playerMovement.UnlockDoubleJump();

        // Felicitación
        npc.ShowMissionCompleted();

        // Opcional: volver al lado del NPC
        // playerMovement.transform.position = npc.transform.position + Vector3.right * 2;
    }
}

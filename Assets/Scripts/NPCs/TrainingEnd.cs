using UnityEngine;

public class TrainingEnd : MonoBehaviour
{
    public Collares mission;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("¡Pista completada!");

        if (mission != null)
            mission.CompleteQuest();
    }
}

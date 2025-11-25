using UnityEngine;

public class EndGameZone : MonoBehaviour
{
    public EndGameManager endGameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (endGameManager != null)
        {
            endGameManager.ShowEndScreen();
        }
        else
        {
            Debug.LogWarning("EndGameTriggerZone: no hay EndGameManager asignado.");
        }
    }
}

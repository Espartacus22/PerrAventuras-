using UnityEngine;

public class Checkpoint : MonoBehaviour
{   
    private void OnTriggerEnter(Collider other)
    {
        var respawn = other.GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.SetCheckpoint(transform.position, transform.rotation);
        }
    }
}

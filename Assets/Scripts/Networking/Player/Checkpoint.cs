using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Opcional: punto exacto de respawn")]
    public Transform spawnPoint;

    [Header("Feedback visual")]
    public GameObject activeVfx;
    public GameObject inactiveVfx;

    bool isActive = false;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        SetVisual(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.SetCheckpoint(this);
            isActive = true;
            SetVisual(true);
        }
    }

    public Vector3 GetSpawnPosition()
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        return transform.position;
    }

    void SetVisual(bool active)
    {
        if (activeVfx != null)
            activeVfx.SetActive(active);
        if (inactiveVfx != null)
            inactiveVfx.SetActive(!active);
    }
}

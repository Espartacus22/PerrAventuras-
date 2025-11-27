using UnityEngine;

public class EnergyCore : MonoBehaviour
{
    [Tooltip("Torretas que dependen de este nucleo.")]
    public LaserTurret[] linkedTurrets;

    [Header("Opcional VFX")]
    public GameObject activeVfx;
    public GameObject destroyedVfx;

    bool isDestroyed = false;

    void Start()
    {
        SetActiveVisual(true);
    }

    // Llamado desde EnemyStats cuando este enemigo muere
    public void OnCoreDestroyed()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log("EnergyCore destruido, apagando torretas…");

        // Apagar torretas
        foreach (var t in linkedTurrets)
        {
            if (t == null) continue;

            // Apagar lógica
            t.DisableTurret();

            // Seguridad extra: desactivar el GO
            // t.gameObject.SetActive(false);
        }

        // Visuales
        SetActiveVisual(false);
        if (destroyedVfx != null)
        {
            Instantiate(destroyedVfx, transform.position, Quaternion.identity);
        }
    }

    void SetActiveVisual(bool active)
    {
        if (activeVfx != null) activeVfx.SetActive(active);
    }
}

using UnityEngine;

public class EnergyCore : MonoBehaviour
{
    [Tooltip("Torretas que dependen de este núcleo.")]
    public MonoBehaviour[] linkedTurrets; // Nota: Si tienes el script LaserTurret, cambia MonoBehaviour por LaserTurret

    [Header("Opcional VFX")]
    public GameObject activeVfx;
    public GameObject destroyedVfx;

    private bool isDestroyed = false;

    void Start()
    {
        SetActiveVisual(true);
    }

    // ¡ESTE ES EL MÉTODO QUE UNITY ESTABA BUSCANDO!
    public void OnCoreDestroyed()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log("EnergyCore destruido, apagando torretas...");

        // Apagar torretas
        foreach (var t in linkedTurrets)
        {
            if (t == null) continue;

            // Si tienes un método específico en tus torretas como DisableTurret(), 
            // asegúrate de llamarlo aquí. Por ahora, apagamos el objeto para que no tire errores.
            t.gameObject.SetActive(false);
        }

        // Actualizar visuales
        SetActiveVisual(false);

        if (destroyedVfx != null)
        {
            // NOTA MULTIJUGADOR: Como las explosiones y chispas son solo efectos visuales que no 
            // afectan la lógica del juego, usar Instantiate normal aquí es PERFECTO porque ahorra 
            // recursos del servidor (cada computadora dibuja su propia explosión).
            Instantiate(destroyedVfx, transform.position, Quaternion.identity);
        }
    }

    void SetActiveVisual(bool active)
    {
        if (activeVfx != null)
        {
            activeVfx.SetActive(active);
        }
    }
}
using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [Header("Parpadeo al recibir daño")]
    public Renderer[] renderers;        // Arrastrar TODOS los MeshRenderer del modelo aquí (piel, ropa, arma, etc.)
    public Color flashColor = Color.red;
    public float flashDuration = 0.08f;
    public int flashCount = 4;

    private Color[] originalColors;
    private Coroutine flashCoroutine;

    void Awake()
    {
        // Guardar colores originales
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
                originalColors[i] = renderers[i].material.color;
        }
    }

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // Poner rojo
            foreach (Renderer r in renderers)
                if (r != null) r.material.color = flashColor;

            yield return new WaitForSeconds(flashDuration);

            // Volver al color original
            for (int j = 0; j < renderers.Length; j++)
                if (renderers[j] != null) renderers[j].material.color = originalColors[j];

            yield return new WaitForSeconds(flashDuration);
        }
    }
}

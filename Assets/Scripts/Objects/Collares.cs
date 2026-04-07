using TMPro;
using UnityEngine;

public class Collares : MonoBehaviour
{
    [Header("Config Quest")]
    public string objectiveTag = "Objetivo";

    [TextArea]
    public string missionDescription = "Obten los COLLARES.";
    public bool autoStart = false;

    [Header("UI")]
    public TextMeshProUGUI textMiss;
    public GameObject buttonMiss;   // Boton de continuar / cerrar mision

    [Header("Opcional")]
    public NPCs questGiver;   // NPC que da la mision (para notificarle al completar)

    // >>> NUEVO BLOQUE <<<
    [Header("Training / Pista (opcional)")]
    [Tooltip("Padre de todas las plataformas, start, end, etc.")]
    public GameObject trainingZone;

    [Tooltip("Punto donde empieza la pista (StartPoint)")]
    public Transform trainingStartPoint;

    [Tooltip("Punto al que vuelve el jugador al terminar (ReturnP_NPC)")]
    public Transform returnPointNPC;

    [Tooltip("Transform del Player")]
    public Transform player;

    private Rigidbody playerRb;
    // >>> FIN NUEVO BLOQUE <<<

    private int numCurrency;
    private bool questActive;
    private bool questCompleted;

    void Start()
    {
        if (trainingZone != null)
            trainingZone.SetActive(false);

        if (buttonMiss != null)
            buttonMiss.SetActive(false);

        // cache del rigidbody del player (si existe)
        if (player != null)
            playerRb = player.GetComponent<Rigidbody>();

        if (autoStart)
        {
            StartQuest();
        }
        else
        {
            // Si no arranca solo, dejamos el texto vacio o algo neutro
            if (textMiss != null)
                textMiss.text = "";
        }
    }

    /// <summary>
    /// Llamar desde el NPC cuando el jugador acepta la mision.
    /// </summary>
    public void StartQuest()
    {
        questActive = true;
        questCompleted = false;

        // Cuenta solo los objetivos con el tag configurado
        numCurrency = GameObject.FindGameObjectsWithTag(objectiveTag).Length;
        UpdateMissionText();

        if (numCurrency == 0)
        {
            Debug.LogWarning($"La misión '{missionDescription}' no tiene objetivos con el tag {objectiveTag}.");
        }

        // activar pista y mandar al inicio
        if (trainingZone != null)
            trainingZone.SetActive(true);

        if (player != null && trainingStartPoint != null)
        {
            player.position = trainingStartPoint.position;
            ResetPlayerVelocity();
        }

        if (questGiver != null)
            questGiver.HideDialogue();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!questActive || questCompleted)
            return;

        if (col.CompareTag(objectiveTag))
        {
            Destroy(col.gameObject);
            numCurrency--;
            UpdateMissionText();

            if (numCurrency <= 0)
            {
                CompleteQuest();
            }
        }
    }

    private void HideQuestTrigger()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            rend.enabled = false;
    }

    public void CompleteQuest()
    {
        questCompleted = true;

        if (textMiss != null)
            textMiss.text = $"{missionDescription} - Misión completada";

        if (buttonMiss != null)
            buttonMiss.SetActive(true);

        // Ocultar pista y devolver al NPC
        if (trainingZone != null)
            trainingZone.SetActive(false);

        if (player != null && returnPointNPC != null)
        {
            player.position = returnPointNPC.position;
            ResetPlayerVelocity();
        }

        // Avisar al NPC que la mision termino (opcional)
        if (questGiver != null)
        {
            questGiver.OnQuestCompleted();
        }

        HideQuestTrigger();

    }

    private void UpdateMissionText()
    {
        if (textMiss != null)
        {
            textMiss.text = $"{missionDescription} Restantes: {numCurrency}";
        }
    }

    // >>> NUEVO helper para no arrastrar velocidad rara <<<
    private void ResetPlayerVelocity()
    {
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }
}

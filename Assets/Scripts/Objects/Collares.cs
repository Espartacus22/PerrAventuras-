using TMPro;
using UnityEngine;

public class Collares : MonoBehaviour
{
    [Header("Config Quest")]
    [Tooltip("Tag de los objetivos de ESTA misión (ej: Objetivo_PruebaMov)")]
    public string objectiveTag = "Objetivo";

    [TextArea]
    public string missionDescription = "Obtén las esferas rojas.";

    [Tooltip("Si está activo, la misión se inicia sola en Start")]
    public bool autoStart = false;

    [Header("UI")]
    public TextMeshProUGUI textMiss;
    public GameObject buttonMiss;   // Botón de continuar / cerrar misión

    [Header("Opcional")]
    public NPClogical questGiver;   // NPC que da la misión (para notificarle al completar)

    private int numCurrency;
    private bool questActive;
    private bool questCompleted;

    void Start()
    {
        if (buttonMiss != null)
            buttonMiss.SetActive(false);

        if (autoStart)
        {
            StartQuest();
        }
        else
        {
            // Si no arranca solo, dejamos el texto vacío o algo neutro
            if (textMiss != null)
                textMiss.text = "";
        }
    }

    /// <summary>
    /// Llamar desde el NPC cuando el jugador acepta la misión.
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

    private void CompleteQuest()
    {
        questCompleted = true;

        if (textMiss != null)
            textMiss.text = $"{missionDescription} - Misión completada";

        if (buttonMiss != null)
            buttonMiss.SetActive(true);

        // Avisar al NPC que la misión terminó (opcional)
        if (questGiver != null)
        {
            questGiver.OnQuestCompleted();
        }
    }

    private void UpdateMissionText()
    {
        if (textMiss != null)
        {
            textMiss.text = $"{missionDescription} Restantes: {numCurrency}";
        }
    }
}

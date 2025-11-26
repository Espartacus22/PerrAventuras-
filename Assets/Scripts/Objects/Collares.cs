using TMPro;
using UnityEngine;

public class Collares : MonoBehaviour
{
    [Header("Config Quest")]
    [Tooltip("Tag de los objetivos de ESTA misi�n (ej: Objetivo_PruebaMov)")]
    public string objectiveTag = "Objetivo";

    [TextArea]
    public string missionDescription = "Obten los COLLARES.";

    [Tooltip("Si est� activo, la mision se inicia sola en Start")]
    public bool autoStart = false;

    [Header("UI")]
    public TextMeshProUGUI textMiss;
    public GameObject buttonMiss;   // Boton de continuar / cerrar mision

    [Header("Opcional")]
    public NPCs questGiver;   // NPC que da la misi�n (para notificarle al completar)

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
            // Si no arranca solo, dejamos el texto vac�o o algo neutro
            if (textMiss != null)
                textMiss.text = "";
        }
    }

    /// <summary>
    /// Llamar desde el NPC cuando el jugador acepta la misi�n.
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
            Debug.LogWarning($"La misi�n '{missionDescription}' no tiene objetivos con el tag {objectiveTag}.");
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
            textMiss.text = $"{missionDescription} - Misi�n completada";

        if (buttonMiss != null)
            buttonMiss.SetActive(true);

        // Avisar al NPC que la misi�n termin� (opcional)
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

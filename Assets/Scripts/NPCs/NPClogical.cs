using TMPro;
using UnityEngine;

public class NPClogical : MonoBehaviour
{
    [Header("Quest")]
    public Collares quest;          // Referencia al script Collares (esta misión)
    public GameObject[] goals;      // Objetivos que se activan al aceptar (si los manejás por objeto)
    public int numGoals;

    [Header("UI")]
    public GameObject missSymbol;   // Icono sobre el NPC
    public GameObject panelNPC;     // Panel de diálogo principal (¿Quieres ayudarme?)
    public GameObject panelNPC2;    // Panel secundario (ej: “Vuelve cuando quieras ayudar”)
    public GameObject panelMiss;    // Panel de misión activa
    public TextMeshProUGUI textMiss;

    [Header("Player")]
    public PlayerMovement player;
    public bool playerClose;
    public bool acceptMiss;         // ¿Ya aceptó la misión?

    void Start()
    {
        // Contar objetivos, si los usás
        if (goals != null)
            numGoals = goals.Length;

        // Buscar player si no está asignado
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerMovement>();
                if (player == null)
                {
                    Debug.LogError("El componente PlayerLocal no se encontró en el objeto con tag 'Player'.");
                }
            }
            else
            {
                Debug.LogError("No se encontró un GameObject con la etiqueta 'Player'.");
            }
        }

        // Enlazar este NPC como questGiver
        if (quest != null)
        {
            quest.questGiver = this;

            // Texto inicial de la misión en el panel, si querés mostrar algo
            if (textMiss != null)
                textMiss.text = quest.missionDescription;
        }

        // Estados iniciales de UI
        if (missSymbol != null) missSymbol.SetActive(true);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(false);
    }

    void Update()
    {
        // Si el jugador está cerca, no aceptó la misión y aprieta E
        if (playerClose && !acceptMiss && Input.GetKeyDown(KeyCode.E) && player != null && player.IsGrounded)
        {
            // Hacer que mire al NPC
            Vector3 positionPlayer = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
            player.transform.LookAt(positionPlayer);

            // Podrías frenar movimiento si querés
            // player.enabled = false;

            // Mostrar panel principal de diálogo
            if (panelNPC != null) panelNPC.SetActive(true);
            if (panelNPC2 != null) panelNPC2.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = true;

            // Si todavía no aceptó la misión, mostrar un “hint”
            if (!acceptMiss && panelNPC2 != null)
            {
                panelNPC2.SetActive(true);   // Ej: “Presiona E para hablar”
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = false;

            if (panelNPC != null) panelNPC.SetActive(false);
            if (panelNPC2 != null) panelNPC2.SetActive(false);
        }
    }

    // Botón NO en el diálogo
    public void NO()
    {
        if (player != null) player.enabled = true;

        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(true); // Ej: “Vuelve si cambias de opinión”
    }

    // Botón YES en el diálogo
    public void YES()
    {
        if (player != null) player.enabled = true;

        acceptMiss = true;

        // Activar los objetivos de la misión
        if (goals != null)
        {
            for (int i = 0; i < goals.Length; i++)
            {
                if (goals[i] != null)
                    goals[i].SetActive(true);
            }
        }

        // Iniciar el quest de recolección
        if (quest != null)
        {
            quest.StartQuest();
        }

        playerClose = false;

        if (missSymbol != null) missSymbol.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(true);
    }

    /// <summary>
    /// Llamado desde Collares cuando la misión se completa.
    /// </summary>
    public void OnQuestCompleted()
    {
        // Acá podés:
        // - Cambiar diálogos
        // - Dar recompensa
        // - Habilitar siguiente misión, etc.

        Debug.Log("Misión completada: el NPC puede dar la recompensa.");

        // Ejemplo simple: ocultar panelMiss o mostrar otro panel
        // if (panelMiss != null) panelMiss.SetActive(false);
    }
}

using TMPro;
using UnityEngine;

public class NPCs : MonoBehaviour
{
    [Header("Quest")]
    public Collares quest;           // Referencia a la misión Collares
    public GameObject[] goals;       // Objetivos que se activan al aceptar (opcional)
    public int numGoals;

    [Header("UI")]
    public GameObject missSymbol;    // Ícono sobre el NPC
    public GameObject panelNPC;      // Panel de diálogo principal (¿Quieres ayudarme?)
    public GameObject panelNPC2;     // Panel secundario (ej: “Pulsa E para hablar” / “Vuelve luego”)
    public GameObject panelMiss;     // Panel de misión activa
    public TextMeshProUGUI textMiss; // Texto de la misión
    public GameObject buttonMiss;    // Botón para cerrar/continuar misión (lo puede usar Collares)

    [Header("Player")]
    public PlayerMovement player;    // Tu script real de movimiento del jugador
    public bool playerClose;         // ¿El jugador está en el trigger?
    public bool acceptMiss;          // ¿Aceptó la misión?
    public float moveSpeed = 3f;     // Velocidad de avance del NPC cuando la misión está activa

    [Header("Tutorial (opcional)")]
    [Tooltip("Mensajes que se muestran en el panel de misión. Si está vacío, se usa un texto por defecto.")]
    public string[] instructions;

    private int currentStep = 0;

    private void Start()
    {
        // Contar objetivos si se usan
        if (goals != null)
            numGoals = goals.Length;

        // Texto por defecto si no cargaste nada en el inspector
        if (instructions == null || instructions.Length == 0)
        {
            instructions = new string[]
            {
                "Presiona W para avanzar y recoge el collar",
                "Usa A y D para moverte a los lados",
                "Salta con ESPACIO",
                "Usa el ratón para mirar alrededor",
                "¡Último collar!"
            };
        }

        // Buscar Player si no está asignado
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerMovement>();
                if (player == null)
                {
                    Debug.LogError("No se encontró PlayerMovement en el objeto con tag 'Player'.");
                }
            }
            else
            {
                Debug.LogError("No se encontró un GameObject con la etiqueta 'Player'.");
            }
        }

        // Enlazar este NPC como questGiver de Collares
        if (quest != null)
        {
            quest.questGiver = this;

            if (textMiss != null)
                textMiss.text = quest.missionDescription;
        }

        // Estado inicial de UI
        if (missSymbol != null) missSymbol.SetActive(true);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(false);
        if (buttonMiss != null) buttonMiss.SetActive(false);
    }

    private void Update()
    {
        // Si el jugador está cerca, no aceptó la misión y toca E
        if (playerClose && !acceptMiss && Input.GetKeyDown(KeyCode.E) && player != null && player.isGrounded)
        {
            // Hacer que mire al NPC
            Vector3 positionPlayer = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
            player.transform.LookAt(positionPlayer);

            // Bloqueamos movimiento mientras habla (si querés)
            player.enabled = false;

            // Mostrar panel principal de diálogo
            if (panelNPC != null) panelNPC.SetActive(true);
            if (panelNPC2 != null) panelNPC2.SetActive(false);
        }

        // Si la misión está activa, el NPC avanza
        if (acceptMiss)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = true;

            // Hint de “pulsa E para hablar”
            if (!acceptMiss && panelNPC2 != null)
            {
                panelNPC2.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = false;

            if (player != null)
                player.enabled = true;

            if (panelNPC != null) panelNPC.SetActive(false);
            if (panelNPC2 != null) panelNPC2.SetActive(false);
        }
    }

    // Botón NO en el diálogo
    public void NO()
    {
        if (player != null) player.enabled = true;

        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(true); // “Vuelve si cambias de opinión”
    }

    // Botón YES en el diálogo
    public void YES()
    {
        if (player != null) player.enabled = true;

        acceptMiss = true;
        currentStep = 0;
        UpdateMissionText();

        // Activar objetivos de escena si los usás
        if (goals != null)
        {
            for (int i = 0; i < goals.Length; i++)
            {
                if (goals[i] != null)
                    goals[i].SetActive(true);
            }
        }

        // Iniciar la misión de Collares
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

    // Actualiza el texto de la misión en el panel
    private void UpdateMissionText()
    {
        if (textMiss == null)
            return;

        if (quest != null && instructions != null && instructions.Length > 0 && currentStep < instructions.Length)
        {
            textMiss.text = instructions[currentStep] + "\n\n" + quest.missionDescription;
        }
        else if (quest != null)
        {
            textMiss.text = quest.missionDescription;
        }
    }

    // Si quisieras avanzar pasos de tutorial desde otro lado
    public void AdvanceStep()
    {
        currentStep++;
        if (currentStep >= instructions.Length)
            currentStep = instructions.Length - 1;

        UpdateMissionText();
    }

    /// <summary>
    /// Llamado desde Collares cuando la misión se completa.
    /// </summary>
    public void OnQuestCompleted()
    {
        Debug.Log("Misión completada: el NPC puede dar la recompensa.");

        if (textMiss != null)
            textMiss.text = "¡Misión completada!";

        if (buttonMiss != null)
            buttonMiss.SetActive(true);
    }
}

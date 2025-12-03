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
    public GameObject panelHintFar;     // "Hey, hey! Óyeme..."
    public GameObject panelHintNear;
    public GameObject panelNPC;      // Panel de diálogo principal (¿Quieres ayudarme?)
    public GameObject panelNPC2;     // Panel secundario (ej: “Vuelve luego”)
    public GameObject panelMiss;     // Panel de misión activa
    public TextMeshProUGUI textMiss; // Texto de la misión
    public GameObject buttonMiss;    // Botón para cerrar/continuar misión

    [Header("Ranges")]
    public float outerRange = 3f;
    public float innerRange = 1.5f;

    [Header("Player")]
    public PlayerMovement player;    // Script real de movimiento del jugador
    public float moveSpeed = 3f;     // Velocidad del NPC cuando la misión está activa

    [Header("Tutorial (opcional)")]
    public string[] instructions;

    [Header("Training Mission")]
    public GameObject trainingZone;        // La pista entera
    public Transform trainingStartPoint;   // Donde aparece el player al aceptar mision
    public Transform trainingEndPoint;     // Opcional: punto donde vuelve después

    [SerializeField] private KeyCode interactKey = KeyCode.E;

    // ESTADO INTERNO
    bool playerInRange = false;  // ¿El jugador está dentro del trigger?
    bool acceptMiss = false;     // ¿Aceptó la misión?
    int currentStep = 0;

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
        if (panelHintFar != null) panelHintFar.SetActive(false);
    }

    private void Update()
    {
        if (player != null)
        {
            // Distancia plano entre NPC y Player
            float dist = Vector3.Distance(player.transform.position, transform.position);

            // Interacción: si está dentro del rango de "Presiona E...", ya puede hablar
            if (!acceptMiss && dist <= innerRange && Input.GetKeyDown(interactKey))
            {
                // Hacer que mire al NPC (solo en XZ)
                Vector3 lookPos = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
                player.transform.LookAt(lookPos);

                // Opcional: bloquear movimiento mientras habla
                player.enabled = false;

                OpenDialogue();
            }

            // Hints según distancia
            if (panelHintFar != null || panelHintNear != null)
            {
                // Muy cerca -> "Presioná E para interactuar"
                if (dist <= innerRange)
                {
                    if (panelHintNear != null) panelHintNear.SetActive(true);
                    if (panelHintFar != null) panelHintFar.SetActive(false);
                }
                // Cerca -> "Hey, hey… ¡Óyeme!"
                else if (dist <= outerRange)
                {
                    if (panelHintFar != null) panelHintFar.SetActive(true);
                    if (panelHintNear != null) panelHintNear.SetActive(false);
                }
                // Lejos -> nada
                else
                {
                    if (panelHintFar != null) panelHintFar.SetActive(false);
                    if (panelHintNear != null) panelHintNear.SetActive(false);
                }
            }
        }

        // Si la misión está activa, el NPC se mueve hacia adelante
        if (acceptMiss && moveSpeed > 0f)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }
    }

    void OpenDialogue()
    {
        if (panelHintFar != null) panelHintFar.SetActive(false);
        if (panelHintNear != null) panelHintNear.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(true);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Solo mostramos hint si todavía no aceptó la misión
        if (!acceptMiss && panelHintFar != null)
            panelHintFar.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (panelHintFar != null) panelHintFar.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);

        // Asegurarnos de que el player vuelva a tener control
        if (player != null) player.enabled = true;
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

        if (missSymbol != null) missSymbol.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(true);
        if (panelHintFar != null) panelHintFar.SetActive(false);
    }
    public void AcceptMission()
    {
        // Mostrar panelNPC y ocultar hint
        if (panelNPC != null) panelNPC.SetActive(true);
        if (panelHintFar != null) panelHintFar.SetActive(false);

        // Activar pista
        if (trainingZone != null)
            trainingZone.SetActive(true);

        // Mover al player a la pista
        if (trainingStartPoint != null)
        {
            player.transform.position = trainingStartPoint.position;
        }
    }

    public void ShowMissionCompleted()
    {
        // Mostrar panelNPC con texto de “¡Felicitaciones!”
        if (panelNPC != null)
            panelNPC.SetActive(true);

        // Cambiar texto...
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

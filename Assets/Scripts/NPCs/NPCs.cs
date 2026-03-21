using TMPro;
using UnityEngine;

public class NPCs : MonoBehaviour
{
    [Header("Quest")]
    public Collares quest;
    public GameObject[] goals;
    public int numGoals;

    [Header("UI")]
    public GameObject missSymbol;
    public GameObject panelHintFar;
    public GameObject panelHintNear;
    public GameObject panelNPC;
    public GameObject panelNPC2;
    public GameObject panelMiss;
    public TextMeshProUGUI textMiss;
    public GameObject buttonMiss;

    [Header("Ranges")]
    public float outerRange = 3f;
    public float innerRange = 1.5f;

    [Header("Player")]
    public PlayerMovement player;
    public float moveSpeed = 3f;

    [Header("Tutorial (opcional)")]
    public string[] instructions;

    [Header("Training Mission")]
    public GameObject trainingZone;
    public Transform trainingStartPoint;
    public Transform trainingEndPoint;

    [Header("Reward")]
    public bool givesDoubleJump = true;
    private bool rewardGiven = false;

    [SerializeField] private KeyCode interactKey = KeyCode.E;

    bool playerInRange = false;
    bool acceptMiss = false;
    int currentStep = 0;

    private void Start()
    {
        if (goals != null)
            numGoals = goals.Length;

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

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerMovement>();
                if (player == null)
                    Debug.LogError("No se encontró PlayerMovement en el objeto con tag 'Player'.");
            }
            else
            {
                Debug.LogError("No se encontró un GameObject con la etiqueta 'Player'.");
            }
        }

        if (quest != null)
        {
            quest.questGiver = this;

            if (textMiss != null)
                textMiss.text = quest.missionDescription;
        }

        if (missSymbol != null) missSymbol.SetActive(true);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(false);
        if (buttonMiss != null) buttonMiss.SetActive(false);
        if (panelHintFar != null) panelHintFar.SetActive(false);
        if (panelHintNear != null) panelHintNear.SetActive(false);
    }

    private void Update()
    {
        if (player != null)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);

            if (!acceptMiss && dist <= innerRange && Input.GetKeyDown(interactKey))
            {
                Vector3 lookPos = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
                player.transform.LookAt(lookPos);
                player.enabled = false;
                OpenDialogue();
            }

            if (!acceptMiss && (panelHintFar != null || panelHintNear != null))
            {
                if (dist <= innerRange)
                {
                    if (panelHintNear != null) panelHintNear.SetActive(true);
                    if (panelHintFar != null) panelHintFar.SetActive(false);
                }
                else if (dist <= outerRange)
                {
                    if (panelHintFar != null) panelHintFar.SetActive(true);
                    if (panelHintNear != null) panelHintNear.SetActive(false);
                }
                else
                {
                    if (panelHintFar != null) panelHintFar.SetActive(false);
                    if (panelHintNear != null) panelHintNear.SetActive(false);
                }
            }
        }

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

        if (!acceptMiss && panelHintFar != null)
            panelHintFar.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (panelHintFar != null) panelHintFar.SetActive(false);
        if (panelHintNear != null) panelHintNear.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);

        if (player != null) player.enabled = true;
    }

    public void NO()
    {
        if (player != null) player.enabled = true;

        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(true);
    }

    public void YES()
    {
        if (player != null) player.enabled = true;

        acceptMiss = true;
        currentStep = 0;
        UpdateMissionText();

        if (goals != null)
        {
            for (int i = 0; i < goals.Length; i++)
            {
                if (goals[i] != null)
                    goals[i].SetActive(true);
            }
        }

        if (quest != null)
        {
            quest.StartQuest();
        }

        if (trainingZone != null)
            trainingZone.SetActive(true);

        if (trainingStartPoint != null && player != null)
        {
            player.transform.position = trainingStartPoint.position;
        }

        if (missSymbol != null) missSymbol.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(true);
        if (panelHintFar != null) panelHintFar.SetActive(false);
        if (panelHintNear != null) panelHintNear.SetActive(false);
    }

    private void UpdateMissionText()
    {
        if (textMiss == null) return;

        if (quest != null && instructions != null && instructions.Length > 0 && currentStep < instructions.Length)
        {
            textMiss.text = instructions[currentStep] + "\n\n" + quest.missionDescription;
        }
        else if (quest != null)
        {
            textMiss.text = quest.missionDescription;
        }
    }

    public void AdvanceStep()
    {
        currentStep++;
        if (currentStep >= instructions.Length)
            currentStep = instructions.Length - 1;

        UpdateMissionText();
    }

    public void OnQuestCompleted()
    {
        Debug.Log("Misión completada: el NPC da la recompensa.");

        if (givesDoubleJump && !rewardGiven && player != null)
        {
            player.UnlockDoubleJump();
            rewardGiven = true;
        }

        if (textMiss != null)
            textMiss.text = "¡Misión completada! Recompensa obtenida: doble salto.";

        if (buttonMiss != null)
            buttonMiss.SetActive(true);

        if (trainingEndPoint != null && player != null)
        {
            player.transform.position = trainingEndPoint.position;
        }
    }

    public void HideDialogue()
    {
        if (panelHintFar != null) panelHintFar.SetActive(false);
        if (panelHintNear != null) panelHintNear.SetActive(false);
        if (panelNPC != null) panelNPC.SetActive(false);
        if (panelNPC2 != null) panelNPC2.SetActive(false);
        if (panelMiss != null) panelMiss.SetActive(false);

        if (player != null) player.enabled = true;
    }
}

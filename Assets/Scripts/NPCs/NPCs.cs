using UnityEngine;

public class NPCs : MonoBehaviour
{
    public GameObject missSymbol;
    public PlayerLocal player;
    public GameObject panelNPC;
    public GameObject panelNPC2;
    public GameObject panelMiss;
    public TextMeshProUGUI textMiss;
    public GameObject buttonMiss;
    public GameObject[] goals; // Asigna 5 esferas inactivas
    public float moveSpeed = 3f; // Velocidad de avance del NPC

    private string[] instructions;
    private int currentStep = 0;
    public bool playerClose;
    public bool acceptMiss;

    void Start()
    {
        // Instrucciones del tutorial (cámbialas en inspector si prefieres array público)
        instructions = new string[]
        {
            "Presiona W para avanzar y recoge el collar",
            "Usa A y D para moverte a los lados",
            "Salta con ESPACIO",
            "Usa el ratón para mirar alrededor",
            "¡Último collar!"
        };

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<PlayerLocal>();
            Collares collares = playerObject.GetComponent<Collares>();
            if (collares != null)
            {
                collares.npc = this;
            }
            else
            {
                Debug.LogError("Collares no encontrado en el Player");
            }
            if (player == null)
            {
                Debug.LogError("PlayerLocal no encontrado en el Player");
            }
        }
        else
        {
            Debug.LogError("No se encontró el Player");
        }

        missSymbol.SetActive(true);
        panelNPC.SetActive(false);
        panelNPC2.SetActive(false);
        panelMiss.SetActive(false);
    }

    void Update()
    {
        if (acceptMiss)
        {
            // NPC avanza automáticamente
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = true;
            if (!acceptMiss)
            {
                // Hacer que el player mire al NPC
                Vector3 npcPosFlat = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);
                other.transform.LookAt(npcPosFlat);
                panelNPC.SetActive(true);
                player.enabled = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.enabled = true;
            playerClose = false;
            if (!acceptMiss)
            {
                panelNPC.SetActive(false);
                panelNPC2.SetActive(true);
            }
        }
    }

    public void NO()
    {
        player.enabled = true;
        panelNPC2.SetActive(false);
        panelNPC.SetActive(true);
    }

    public void YES()
    {
        player.enabled = true;
        acceptMiss = true;

        // Desactivar todos los collares por si acaso
        foreach (GameObject goal in goals)
        {
            goal.SetActive(false);
        }

        currentStep = 0;
        ActivateGoal(currentStep);
        UpdateUI();
        playerClose = false;
        missSymbol.SetActive(false);
        panelNPC.SetActive(false);
        panelNPC2.SetActive(false);
        panelMiss.SetActive(true);
    }

    private void ActivateGoal(int index)
    {
        if (index < goals.Length)
        {
            goals[index].SetActive(true);
        }
    }

    private void UpdateUI()
    {
        if (currentStep < instructions.Length)
        {
            textMiss.text = instructions[currentStep] + "\nCollares restantes: " + (goals.Length - currentStep);
        }
    }

    public void OnCollarCollected()
    {
        currentStep++;
        if (currentStep < goals.Length)
        {
            ActivateGoal(currentStep);
            UpdateUI();
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void CompleteTutorial()
    {
        textMiss.text = "¡Misión completada!";
        buttonMiss.SetActive(true);
    }
}

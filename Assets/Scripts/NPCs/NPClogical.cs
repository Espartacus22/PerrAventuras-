using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class NPClogical : MonoBehaviour
{
    public GameObject missSymbol;
    public PlayerLocal player;
    public GameObject panelNPC;
    public GameObject panelNPC2;
    public GameObject panelMiss;
    public TextMeshProUGUI textMiss;
    public bool playerClose;
    public bool acceptMiss;
    public GameObject[] goals;
    public int numGoals;
    public GameObject buttonMiss;

    void Start()
    {
        numGoals = goals.Length;
        textMiss.text = $"Obten los collares. Restantes: {numGoals}";
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<PlayerLocal>();
            if (player == null)
            {
                Debug.LogError("El componente PlayerLocal no se encontró en el objeto con la etiqueta 'Player'.");
            }
        }
        else
        {
            Debug.LogError("No se encontró un GameObject con la etiqueta 'Player'.");
        }
        missSymbol.SetActive(true);
        panelNPC.SetActive(false);
        panelMiss.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !acceptMiss && player.isGrounded)
        {
            Vector3 positionPlayer = new Vector3(transform.position.x, player.gameObject.transform.position.y, transform.position.z);
            player.gameObject.transform.LookAt(positionPlayer);
            //player.anim.SetFloat("VelX", 0);
            //player.anim.SetFloat("VelY", 0);
            //player.enabled = false;
            panelNPC.SetActive(false);
            panelNPC2.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = true;
            if (!acceptMiss)
            {
                panelNPC.SetActive(true);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = false;
            panelNPC.SetActive(false);
            panelNPC2.SetActive(true);
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
        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].SetActive(true);
        }
        playerClose = false;
        missSymbol.SetActive(false);
        panelNPC.SetActive(false);
        panelNPC2.SetActive(false);
        panelMiss.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NPCmision : MonoBehaviour
{
    public GameObject MissIon;
    public logicaPlayer player;
    public GameObject panelNPC;
    public GameObject panelNPC2;
    public GameObject panelNPC3;
    public TextMeshProUGUI TextMiss;
    public bool playerClose;
    public bool AceceptPlayer;
    public GameObject goals;
    public int numGoals;

    void Start()
    {
        numGoals = goals.Length;
        TextMiss.text = "Obten las monedas" + " Restantes" + numGoals;
        player = gameObject.FindGameObjectWithTag("Player").GetComponent<logicalPlayer>();
        MissIon.SetActive(true);
        panelNPC.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && player.jump == true) ;
        {
            Vector3 positionPlayer = new Vector3(transform.position.x, player.GameObject.transform.position.y, transform.position.z);

            player.anim.SetFloat("Volx", 0);
            player.anim.SetFloat("Voly", 0);
            player.enabled = false;
            panelNPC.SetActive(false);
            panelNPC2.SetActive(true);
        }
    }

}

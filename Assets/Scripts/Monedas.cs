using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Monedas : MonoBehaviour
{
    public int numCurrency;
    public TextMeshProUGUI textMiss;
    public GameObject ButtonMiss;
    void Start()
    {
        numCurrency - GameObject.FindGameObjectsWithTag("Objetos").Length;
        textMiss.text = "Obten las esfweras rojas" + " Restantes;" + numCurrency;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "objetivo")
        {
            Destroy(col.gameObject.tag == "objetivo");
            numCurrency--;
            textMiss.text = "Obten las esferas rojas" + " Restantes" + numCurrency;

            if (numCurrency) ;
        }
    }
}

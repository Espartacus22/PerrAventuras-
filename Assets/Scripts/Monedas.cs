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
        // Inicializa numCurrency con la cantidad de objetos con tag "objetivo"
        numCurrency = GameObject.FindGameObjectsWithTag("objetivo").Length;
        UpdateMissionText();
    }

    // Se llama cuando un objeto entra en el trigger
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("objetivo"))
        {
            Destroy(col.gameObject); // Destruye el GameObject que entró en el trigger
            numCurrency--;
            UpdateMissionText();

            if (numCurrency <= 0)
            {
                textMiss.text = "Misión completada";
                ButtonMiss.SetActive(true);
            }
        }
    }

    // Método para actualizar el texto de la misión
    void UpdateMissionText()
    {
        if (textMiss != null)
        {
            textMiss.text = $"Obtén las esferas rojas. Restantes: {numCurrency}";
        }
        else
        {
            Debug.LogError("textMiss no está asignado en el Inspector");
        }
    }
}
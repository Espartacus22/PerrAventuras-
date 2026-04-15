using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] string menuSceneName = "MainMenu"; 

    void Update()
    {
        // Detecta si se presiona la tecla Escape
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Evita que Escape cierre el juego: cargá el menú en su lugar
            SceneManager.LoadScene(menuSceneName);
        }
    }
}

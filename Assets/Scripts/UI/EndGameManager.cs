using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [Header("UI de fin de juego")]
    public GameObject endGameCanvas;   // Canvas con "Continuará..." y botones

    [Header("Opcional: desactivar control del jugador al ganar")]
    public GameObject playerObject;    // Player root, por si querés desactivar su script de movimiento

    private bool _gameEnded = false;

    private void Start()
    {
        if (endGameCanvas != null)
            endGameCanvas.SetActive(false);
    }

    private void Update()
    {
        // ESC para salir en cualquier momento
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }

    /// <summary>
    /// Llamar cuando el player cruza la puerta final
    /// </summary>
    public void ShowEndScreen()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (endGameCanvas != null)
            endGameCanvas.SetActive(true);

        // Desactivar movimiento del player (si querés)
        if (playerObject != null)
        {
            var move1 = playerObject.GetComponent<CharacterController>();
            if (move1 != null) move1.enabled = false;

            // Si tenés otro script de movimiento custom, desactivalo acá:
            // var myMovement = playerObject.GetComponent<MiScriptMovimiento>();
            // if (myMovement != null) myMovement.enabled = false;
        }

        Time.timeScale = 0f; // pausa todo el juego mientras se muestra el menú final
        Debug.Log("Fin del nivel - mostrando pantalla de 'Continuará...'");
    }

    // BOTÓN Retry
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    // BOTÓN Exit + tecla ESC
    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        // Para salir del Play Mode en el editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

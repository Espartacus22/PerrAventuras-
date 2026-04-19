using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Lobby";  
    [SerializeField] string loginSceneName = "AuthService"; 

    async void Start()
    {
        // Inicializa Unity Services si aún no están inicializados
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
    }

    public void OnStartPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnExitPressed()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnLogoutPressed()
    {
        // Cierra la sesión del jugador
        AuthenticationService.Instance.SignOut();

        // Vuelve a la escena de login
        SceneManager.LoadScene(loginSceneName);
    }
}

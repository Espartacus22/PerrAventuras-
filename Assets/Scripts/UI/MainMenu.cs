using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene"; // o el nombre de tu escena principal

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
}

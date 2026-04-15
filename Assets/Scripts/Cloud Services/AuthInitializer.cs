using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AuthInitializer : MonoBehaviour
{
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signUpAccountButton;
    [SerializeField] private Button signInEmailPasswordButton;

    [Header("Email and Password")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorText;

    async void Start()
    {
        signInButton.interactable = false;

        // Inicializar servicios solo si no están inicializados
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        signInButton.interactable = true;

        signInButton.onClick.AddListener(SignInAnonymously);

        signUpAccountButton.onClick.AddListener(() =>
        {
            SignUpWithEmailPasswordAsync(emailInput.text, passwordInput.text);
        });

        signInEmailPasswordButton.onClick.AddListener(() =>
        {
            SignInWithEmailPasswordAsync(emailInput.text, passwordInput.text);
        });
    }

    private async void SignInAnonymously()
    {
        signInButton.gameObject.SetActive(false);
        try
        {
            // Solo loguear si no está ya logueado
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            errorText.text = "";
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);

            SceneManager.LoadScene("MainMenu");
        }
        catch (Exception e)
        {
            errorText.text = "Error al iniciar sesión anónima: " + e.Message;
        }
    }

    private async void SignUpWithEmailPasswordAsync(string email, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(email, password);
            errorText.text = "";
            Debug.Log("Registro exitoso. Player ID: " + AuthenticationService.Instance.PlayerId);

            // Login automático tras registro, solo si no está ya logueado
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
                Debug.Log("Login automático exitoso.");
            }

            SceneManager.LoadScene("MainMenu");
        }
        catch (AuthenticationException ex)
        {
            errorText.text = "Error de autenticación: " + ex.Message;
        }
        catch (RequestFailedException ex)
        {
            errorText.text = "Error de solicitud: " + ex.Message;
        }
    }

    private async void SignInWithEmailPasswordAsync(string email, string password)
    {
        try
        {
            // Solo loguear si no está ya logueado
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            }

            errorText.text = "";
            Debug.Log("Login exitoso. Player ID: " + AuthenticationService.Instance.PlayerId);

            SceneManager.LoadScene("MainMenu");
        }
        catch (AuthenticationException ex)
        {
            errorText.text = "Error de autenticación: " + ex.Message;
        }
        catch (RequestFailedException ex)
        {
            errorText.text = "Error de solicitud: " + ex.Message;
        }
    }

    // Método para Logout (se usa en la escena MainMenu)
    public void Logout()
    {
        AuthenticationService.Instance.SignOut();
        errorText.text = "";
        SceneManager.LoadScene("AuthService");
    }
}

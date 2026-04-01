using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
public class AuthInitializer : MonoBehaviour
{
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signUpAccountButton;
    [SerializeField] private Button signInUserPasswordButton;


    [Header("Username and Password")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    async void Start()
    {
        signInButton.interactable = false;
        await UnityServices.InitializeAsync();
        signInButton.interactable = true;
        signInButton.onClick.AddListener(SignIn);

        signUpAccountButton.onClick.AddListener(() => {
            SignUpWithUsernamePasswordAsync(usernameInput.text, passwordInput.text);
        });


        signInUserPasswordButton.onClick.AddListener(() => {
            SignInWithUsernamePasswordAsync(usernameInput.text, passwordInput.text);
        });
    }

    private async void SignIn()
    {
        signInButton.gameObject.SetActive(false);
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
        }
        catch (Exception e)
        {
            Debug.LogError("Sign in failed: " + e.Message);
            throw;
        }

        Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        await Task.Delay(3000);
        Debug.Log("Sign in successful");
    }


    private async void SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }


    async void SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }


}

// Inicializa Unity Game Services
// Inicia sesión anónima del jugador
// Imprime el Player ID en consola


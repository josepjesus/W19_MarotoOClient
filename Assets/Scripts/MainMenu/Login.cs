using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public Button loginButton;
    public Button logoutButton;
    public Button playGameButton;
    public Text messageBoardText;
    public Player playerManager;

    private string httpServerAddress;

    private void Start()
    {
        httpServerAddress = playerManager.GetHttpServer();
    }

    public void OnLoginButtonClicked()
    {
        TryLogin();
    }

    private void GetToken()
    {
        UnityWebRequest http = new UnityWebRequest(playerManager.GetHttpServer() + "Token", "POST");

        WWWForm dataToSend = new WWWForm();

        dataToSend.AddField("grant_type", "password");
        dataToSend.AddField("username", emailInputField.text);
        dataToSend.AddField("password", passwordInputField.text);

        http.uploadHandler = new UploadHandlerRaw(dataToSend.data);
        http.downloadHandler = new DownloadHandlerBuffer();

        http.SetRequestHeader("Accept", "application/json");

        http.SendWebRequest();

        while (!http.isDone)
        {
            Task.Delay(1);
        }

        if (http.isNetworkError || http.isHttpError)
        {
            Debug.Log(http.error);
        }
        else
        {
            string jsonResponse = http.downloadHandler.text;
            AuthoriationToken authToken = JsonUtility.FromJson<AuthoriationToken>(jsonResponse);
            playerManager.Token = authToken.access_token;

        }

        http.Dispose();
    }

    private void TryLogin()
    {
        if (string.IsNullOrEmpty(playerManager.Token))
        {
            GetToken();
        }
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        else if (string.IsNullOrEmpty(passwordInputField.text))
        {
            throw new NullReferenceException("Password can't be void");
        }
        UnityWebRequest http = new UnityWebRequest(playerManager.GetHttpServer() + "api/Account/UserId", "GET");

        http.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
        http.SetRequestHeader("Accept", "application/json");

        http.downloadHandler = new DownloadHandlerBuffer();
        http.SendWebRequest();
        while (!http.isDone)
        {
            Task.Delay(1);
        }
        Debug.Log(http.isHttpError.ToString());
        Debug.Log(http.isNetworkError.ToString());
        if (http.isNetworkError || http.isHttpError)
        {
            Debug.Log(http.error);
        }
        else
        {
            playerManager.PlayerId = http.downloadHandler.text;
            messageBoardText.text += "\nWelcome " + playerManager.PlayerId + ". You are logged in!";
            loginButton.interactable = false;
            logoutButton.interactable = true;

            playGameButton.interactable = true;
        }

        http.Dispose();
    }

    public void OnLogoutButtonClicked()
    {
        TryLogout();
    }

    private void TryLogout()
    {
        UnityWebRequest http = new UnityWebRequest(playerManager.GetHttpServer() + "api/Account/Logout", "POST");
        http.SetRequestHeader("Authorization", "bearer " + playerManager.Token);
        http.SendWebRequest();

        while (!http.isDone)
        {
            Task.Delay(1);
        }

        Debug.Log(http.isHttpError.ToString());
        Debug.Log(http.isNetworkError.ToString());
        if (http.isNetworkError || http.isHttpError)
        {
            Debug.Log(http.error);
        }
        else
        {
            messageBoardText.text += $"\n{http.responseCode} Bye bye {playerManager.PlayerId}";
            playerManager.Token = string.Empty;
            playerManager.PlayerId = string.Empty;
            loginButton.interactable = true;
            logoutButton.interactable = false;
        }
    }
}

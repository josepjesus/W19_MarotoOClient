using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;

public class Register : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public Button registerButton;
    public Text messageBoardText;
    public Player playerManager;

    string httpServer;

    private void Start()
    {
        httpServer = playerManager.GetHttpServer();
        playerManager = FindObjectOfType<Player>();
    }

    public void OnRegisterButtonClick()
    {
        StartCoroutine(RegisterNewUser());
    }

    public IEnumerator RegisterNewUser()
    {
        
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            throw new NullReferenceException("Email can't be void");
        }
        else if (string.IsNullOrEmpty(passwordInputField.text))
        {
            throw new NullReferenceException("Password can't be void");
        }
        else if (passwordInputField.text != confirmPasswordInputField.text)
        {
            throw new Exception("Passwords don't match");
        }
        UnityWebRequest http = new UnityWebRequest(playerManager.GetHttpServer() + "api/Account/Register", "POST");
        AspNetUserRegister newUser = new AspNetUserRegister();
        newUser.Email = emailInputField.text;
        newUser.Password = passwordInputField.text;
        newUser.ConfirmPassword = confirmPasswordInputField.text;

        string jSon = JsonUtility.ToJson(newUser);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jSon);

        http.uploadHandler = new UploadHandlerRaw(dataToSend);
        http.SetRequestHeader("Content-Type", "application/json");

        yield return http.SendWebRequest();
        Debug.Log(http.isHttpError.ToString());
        Debug.Log(http.isNetworkError.ToString());
        if (http.isNetworkError || http.isHttpError)
        {
            throw new Exception(http.error);
        }


        messageBoardText.text += "\n" + http.responseCode;
        http.Dispose();
    }

}

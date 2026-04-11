using UnityEngine;
using System;

public class GoogleLoginService : MonoBehaviour
{
    [SerializeField] private string clientId = "358666565391-nbats8o0q0bamfstt5590bgjug3qgbp3.apps.googleusercontent.com";
    [SerializeField] private string redirectUri = "https://developers.google.com/oauthplayground";

    public void StartLogin()
    {
        string authUrl =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + clientId +
            "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
            "&response_type=code" +
            "&scope=" + Uri.EscapeUriString("openid email profile");
        Application.OpenURL(authUrl);
    }
}

using System;
using UnityEngine;

public class WebViewOAuthService : MonoBehaviour
{
    //public event Action<string, string> OnAuthorizationCodeReceived;

    [Header("Google")]
    [SerializeField] private string googleClientId;
    [SerializeField] private string googleRedirectUri;

    [Header("Kakao")]
    [SerializeField] private string kakaoRestApiKey;
    [SerializeField] private string kakaoRedirectUri;

    public void StartLogin(string provider)
    {
        string authUrl = BuildAuthorizationUrl(provider);

        if (string.IsNullOrEmpty(authUrl))
        {
            Debug.LogError($"[WebViewOAuthService] Unsupported provider: {provider}");
            return;
        }

        Debug.Log($"[WebViewOAuthService] Open auth url: {authUrl}");

        //OpenWebView(authUrl);
        Application.OpenURL(authUrl);
    }

    private string BuildAuthorizationUrl(string provider)
    {
        provider = provider.ToLower();

        if (provider == "google")
        {
            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + Uri.EscapeDataString(googleClientId)
                + "&redirect_uri=" + Uri.EscapeDataString(googleRedirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile");
        }

        if (provider == "kakao")
        {
            return "https://kauth.kakao.com/oauth/authorize"
                + "?client_id=" + Uri.EscapeDataString(kakaoRestApiKey)
                + "&redirect_uri=" + Uri.EscapeDataString(kakaoRedirectUri)
                + "&response_type=code";
        }

        return null;
    }
}
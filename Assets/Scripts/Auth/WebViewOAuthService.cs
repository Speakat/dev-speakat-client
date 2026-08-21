using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebViewOAuthService : MonoBehaviour
{
    private const string DefaultOAuthCallbackUri = "https://speakatweb.chokoring.com/callback.html";

    [Header("Google")]
    [SerializeField] private string googleClientId;
    [SerializeField] private string googleRedirectUri;

    [Header("Kakao")]
    [SerializeField] private string kakaoRestApiKey;
    [SerializeField] private string kakaoRedirectUri;

    [Header("Shared Callback")]
    [SerializeField] private string oauthCallbackUri = DefaultOAuthCallbackUri;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenOAuthPopup(string url);
#endif

    public void StartLogin(string provider)
    {
        string authUrl = BuildAuthorizationUrl(provider);

        if (string.IsNullOrEmpty(authUrl))
        {
            Debug.LogError($"[WebViewOAuthService] Unsupported provider or empty authUrl: {provider}");
            return;
        }

        Debug.Log($"[WebViewOAuthService] authUrl = {authUrl}");

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[WebViewOAuthService] WebGL 환경: JS 팝업으로 OAuth 페이지를 엽니다.");
        OpenOAuthPopup(authUrl);
#else
        Debug.Log("[WebViewOAuthService] 모바일/에디터 환경: 시스템 브라우저로 OAuth 페이지를 엽니다.");
        Application.OpenURL(authUrl);
#endif
    }

    private string BuildAuthorizationUrl(string provider)
    {
        if (string.IsNullOrEmpty(provider))
        {
            Debug.LogError("[WebViewOAuthService] provider is null or empty");
            return null;
        }

        provider = provider.ToLower();

        string redirectUri = ResolveRedirectUri(provider);
        string encodedState = Uri.EscapeDataString(provider);

        if (provider == "google")
        {
            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + Uri.EscapeDataString(googleClientId)
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile")
                + "&state=" + encodedState;
        }

        if (provider == "kakao")
        {
            return "https://kauth.kakao.com/oauth/authorize"
                + "?client_id=" + Uri.EscapeDataString(kakaoRestApiKey)
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&response_type=code"
                + "&state=" + encodedState;
        }

        Debug.LogError($"[WebViewOAuthService] Unsupported provider: {provider}");
        return null;
    }

    private string ResolveRedirectUri(string provider)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return NormalizeRedirectUri(oauthCallbackUri);
#elif UNITY_ANDROID && !UNITY_EDITOR
        return NormalizeRedirectUri(oauthCallbackUri);
#else
        if (provider == "google" && !string.IsNullOrWhiteSpace(googleRedirectUri))
            return NormalizeRedirectUri(googleRedirectUri);

        if (provider == "kakao" && !string.IsNullOrWhiteSpace(kakaoRedirectUri))
            return NormalizeRedirectUri(kakaoRedirectUri);

        return NormalizeRedirectUri(oauthCallbackUri);
#endif
    }

    private string NormalizeRedirectUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return DefaultOAuthCallbackUri;

        return uri.Trim()
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("\t", "");
    }
}
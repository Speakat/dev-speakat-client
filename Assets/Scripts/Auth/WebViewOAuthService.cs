using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebViewOAuthService : MonoBehaviour
{
    [Header("Google")]
    [SerializeField] private string googleClientId;
    [SerializeField] private string googleRedirectUri;

    [Header("Kakao")]
    [SerializeField] private string kakaoRestApiKey;
    [SerializeField] private string kakaoRedirectUri;

    [Header("WebGL")]
    [SerializeField] private string webGLRedirectUri = "http://127.0.0.1:5500/callback.html";

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
        Debug.Log("[WebViewOAuthService] WEBGL 환경 감지: JS 팝업을 시도");
        OpenOAuthPopup(authUrl);
#else
        Debug.Log("[WebViewOAuthService] 모바일/에디터 환경 감지: 시스템 브라우저 시도");
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

        string currentGoogleRedirectUri = googleRedirectUri;
        string currentKakaoRedirectUri = kakaoRedirectUri;

#if UNITY_WEBGL && !UNITY_EDITOR
        currentGoogleRedirectUri = webGLRedirectUri;
        currentKakaoRedirectUri = webGLRedirectUri;
#endif

        string encodedState = Uri.EscapeDataString(provider);

        if (provider == "google")
        {
            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + Uri.EscapeDataString(googleClientId)
                + "&redirect_uri=" + Uri.EscapeDataString(currentGoogleRedirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile")
                + "&state=" + encodedState;
        }

        if (provider == "kakao")
        {
            return "https://kauth.kakao.com/oauth/authorize"
                + "?client_id=" + Uri.EscapeDataString(kakaoRestApiKey)
                + "&redirect_uri=" + Uri.EscapeDataString(currentKakaoRedirectUri)
                + "&response_type=code"
                + "&state=" + encodedState;
        }

        Debug.LogError($"[WebViewOAuthService] Unsupported provider: {provider}");
        return null;
    }
}
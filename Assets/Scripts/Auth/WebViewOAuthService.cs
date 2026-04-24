using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class WebViewOAuthService : MonoBehaviour
{
    //public event Action<string, string> OnAuthorizationCodeReceived;

    [Header("Google")]
    [SerializeField] private string googleClientId;
    [SerializeField] private string googleRedirectUri;

    [Header("Kakao")]
    [SerializeField] private string kakaoRestApiKey;
    [SerializeField] private string kakaoRedirectUri;

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenOAuthPopup(string url);
    #endif

    public void StartLogin(string provider)
    {
        string authUrl = BuildAuthorizationUrl(provider);
        Debug.Log($"[WebViewOAuthService] authUrl = {authUrl}");

        //if (string.IsNullOrEmpty(authUrl))
        //{
        //    Debug.LogError($"[WebViewOAuthService] Unsupported provider: {provider}");
        //    return;
        //}

        #if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("[WebViewOAuthService] WEBGL 환경 감지: JS 팝업을 시도");
            OpenOAuthPopup(authUrl);
        #else
        Debug.Log("[WebViewOAuthService] 모바일 환경 감지: 시스템 브라우저 시도");
            Application.OpenURL(authUrl);
        #endif

        //Debug.Log($"[WebViewOAuthService] Open auth url: {authUrl}");

        //OpenWebView(authUrl);
        //Application.OpenURL(authUrl);
    }

    private string BuildAuthorizationUrl(string provider)
    {
        provider = provider.ToLower();

        string currentGoogleRedirectUri = googleRedirectUri;
        string currentKakaoRedirectUri = kakaoRedirectUri;

        #if UNITY_WEBGL && !UNITY_EDITOR
            currentGoogleRedirectUri = "http://127.0.0.1:5500/callback.html";
            currentKakaoRedirectUri = "http://127.0.0.1:5500/callback.html";
        #endif

        if (provider == "google")
        {
            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + Uri.EscapeDataString(googleClientId)
                + "&redirect_uri=" + Uri.EscapeDataString(currentGoogleRedirectUri)
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile");
        }

        if (provider == "kakao")
        {
            return "https://kauth.kakao.com/oauth/authorize"
                + "?client_id=" + Uri.EscapeDataString(kakaoRestApiKey)
                + "&redirect_uri=" + Uri.EscapeDataString(currentKakaoRedirectUri)
                + "&response_type=code";
        }

        return null;
    }
}
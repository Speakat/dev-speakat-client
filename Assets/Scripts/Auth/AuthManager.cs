using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private AuthApi authApi;
    [SerializeField] private WebViewOAuthService webViewOAuthService;

    [Header("Deep Link Handler")]
    [SerializeField] private OAuthLinkHandler linkHandler;

    [Header("Scene & UI Routing")]
    [SerializeField] private string nextSceneName = "Stage";
    [SerializeField] private GameObject signupInfoPanel; //newUser용 팝업 패널

    private void OnEnable()
    {
        if (linkHandler != null)
            linkHandler.OnAuthorizationCodeReceived += HandleAuthorizationCode;
    }

    public void OnDisable()
    {
        if (linkHandler != null)
            linkHandler.OnAuthorizationCodeReceived -= HandleAuthorizationCode;
    }

    public void StartGoogleLogin()
    {
        webViewOAuthService.StartLogin("google");
    }

    public void StartMockGoogleLogin()
    {
        OAuthLoginResponse mockResponse = new OAuthLoginResponse
        {
            isSuccess = true,
            data = new OAuthLoginData
            {
                userId = "1",
                email = "test@gmail.com",
                nickname = "GoogleUser",
                profileImageUrl = "https://example.com/profile.png",
                provider = "GOOGLE",
                accessToken = "mock-access-token",
                refreshToken = "mock-refresh-token",
                isNewUser = true
            }
        };

        OnLoginSuccess(mockResponse);
    }

    public void StartKakaoLogin()
    {
        webViewOAuthService.StartLogin("kakao");
    }

    private void HandleAuthorizationCode(string provider, string authorizationCode)
    {
        Debug.LogError($"[AuthManager] Authorization code received, provider={provider}");

        StartCoroutine(authApi.LoginWithOAuth(
            provider, authorizationCode, OnLoginSuccess, OnLoginFail
        ));
    }

    public void OnReceiveCodeFromJS(string codeData)
    {
        int idx = codeData.IndexOf(':');
        if (idx <= 0 || idx >= codeData.Length - 1)
        {
            Debug.LogError("[AuthManager] Invalid code from JS");
            return;
        }

        string provider = codeData.Substring(0, idx);
        string code = codeData.Substring(idx + 1);

        Debug.Log($"[AuthManager] WebGL Code Received: {provider}");
        HandleAuthorizationCode(provider, code);
    }

    private void OnLoginSuccess(OAuthLoginResponse response)
    {
        if (response == null || response.data == null)
        {
            Debug.LogError("[AuthManager] Invalid login response.");
            return;
        }

        Debug.Log("[AuthManager] OnLoginSuccess called");
        TokenStore.Instance.SetLoginData(response.data);

        if (response.data.isNewUser)
        {
            Debug.Log("[AuthManager] newUser: input window open");
            if (signupInfoPanel != null) signupInfoPanel.SetActive(true);
            else
            {
                Debug.LogWarning("[AuthManager] signupInfoPanel이 연결되지 않았습니다.");
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.Log("[AuthManager] 기존 유저. move to next scene");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnLoginFail(string errorMessage)
    {
        Debug.LogError($"[AuthManager] Login failed: {errorMessage}");
    }

    public void GoToNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}

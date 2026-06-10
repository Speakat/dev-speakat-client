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
    [SerializeField] private GameObject signupInfoPanel;
    [SerializeField] private SignupViewController signupViewController;

    private void OnEnable()
    {
        if (linkHandler != null)
            linkHandler.OnAuthorizationCodeReceived += HandleAuthorizationCode;
    }

    private void OnDisable()
    {
        if (linkHandler != null)
            linkHandler.OnAuthorizationCodeReceived -= HandleAuthorizationCode;
    }

    public void StartGoogleLogin()
    {
        if (webViewOAuthService == null)
        {
            Debug.LogError("[AuthManager] WebViewOAuthService is not assigned.");
            return;
        }

        webViewOAuthService.StartLogin("google");
    }

    public void StartKakaoLogin()
    {
        if (webViewOAuthService == null)
        {
            Debug.LogError("[AuthManager] WebViewOAuthService is not assigned.");
            return;
        }

        webViewOAuthService.StartLogin("kakao");
    }

    public void StartMockGoogleLogin()
    {
        OAuthLoginResponse mockResponse = new OAuthLoginResponse
        {
            isSuccess = true,
            data = new OAuthLoginData
            {
                userId = "mock-user-id",
                email = "test@gmail.com",
                nickname = "GoogleUser",
                profileImageUrl = "https://example.com/profile.png",
                provider = "GOOGLE",
                accessToken = "mock-access-token",
                refreshToken = "mock-refresh-token",
                isNewUser = true
            },
            code = null,
            message = null
        };

        OnLoginSuccess(mockResponse);
    }

    private void HandleAuthorizationCode(string provider, string authorizationCode)
    {
        Debug.Log($"[AuthManager] Authorization code received, provider={provider}");

        if (authApi == null)
        {
            Debug.LogError("[AuthManager] AuthApi is not assigned.");
            return;
        }

        StartCoroutine(authApi.LoginWithOAuth(
            provider,
            authorizationCode,
            OnLoginSuccess,
            OnLoginFail
        ));
    }

    public void OnReceiveCodeFromJS(string codeData)
    {
        int idx = codeData.IndexOf(':');

        if (idx <= 0 || idx >= codeData.Length - 1)
        {
            Debug.LogError($"[AuthManager] Invalid code from JS: {codeData}");
            return;
        }

        string provider = codeData.Substring(0, idx);
        string code = codeData.Substring(idx + 1);

        Debug.Log($"[AuthManager] WebGL Code Received: provider={provider}");
        HandleAuthorizationCode(provider, code);
    }

    private void OnLoginSuccess(OAuthLoginResponse response)
    {
        if (response == null)
        {
            Debug.LogError("[AuthManager] Login response is null.");
            return;
        }

        if (!response.isSuccess)
        {
            Debug.LogError($"[AuthManager] Login API failed: code={response.code}, message={response.message}");
            return;
        }

        if (response.data == null)
        {
            Debug.LogError("[AuthManager] Login response data is null.");
            return;
        }

        Debug.Log($"[AuthManager] OnLoginSuccess called. userId={response.data.userId}, nickname={response.data.nickname}, isNewUser={response.data.isNewUser}");

        if (TokenStore.Instance == null)
        {
            Debug.LogError("[AuthManager] TokenStore.Instance is null. Scene에 TokenStore 오브젝트가 있는지 확인 필요");
            return;
        }

        TokenStore.Instance.SetLoginData(response.data);

        if (response.data.isNewUser)
        {
            Debug.Log("[AuthManager] New user: signup info panel open");

            if (signupInfoPanel != null)
            {
                signupInfoPanel.SetActive(true);

                if (signupViewController != null)
                {
                    signupViewController.SetupSocialProfile(
                        response.data.nickname,
                        response.data.profileImageUrl
                    );
                }
                else
                {
                    Debug.LogWarning("[AuthManager] signupViewController is not assigned.");
                }
            }
            else
            {
                Debug.LogWarning("[AuthManager] signupInfoPanel is not assigned. Move to next scene.");
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.Log("[AuthManager] Existing user. Move to next scene.");
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
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("Services")]
    //[SerializeField] private GoogleLoginService googleLoginService;
    //[SerializeField] private KakaoLoginService kakaoLoginService;
    [SerializeField] private AuthApi authApi;
    [SerializeField] private WebViewOAuthService webViewOAuthService;
    //[SerializeField] private TokenStore tokenStore;

    [Header("Deep Link Handler")]
    [SerializeField] private OAuthLinkHandler linkHandler;

    //Mock check button
    //[Header("Mock")]
    //[SerializeField] private bool useMockLogin = true;

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
        //Debug.Log("[AuthManager] StartGoogleLogin");

        //if (useMockLogin)
        //{
        //    LoginWithMockJson("mock_google_existing_user");
        //    return;
        //}
        //googleLoginService.StartLogin();
        webViewOAuthService.StartLogin("google");
    }

    //Google Mock Login test
    //public void StartMockGoogleLogin()
    //{
    //    OAuthLoginResponse mockResponse = new OAuthLoginResponse
    //    {
    //        isSuccess = true,
    //        data = new OAuthLoginData
    //        {
    //            userId = "1",
    //            email = "test@gmail.com",
    //            nickname = "GoogleUser",
    //            profileImageUrl = "https://example.com/profile.png",
    //            provider = "GOOGLE",
    //            accessToken = "mock-access-token",
    //            refreshToken = "mock-refresh-token",
    //            isNewUser = true
    //        }
    //    };

    //    OnLoginSuccess(mockResponse);
    //}

    public void StartKakaoLogin()
    {
        //Debug.Log("[AuthManager] StartKakaoLogin");

        //if (useMockLogin)
        //{
        //    LoginWithMockJson("mock_kakao_login");
        //    return;
        //}
        //kakaoLoginService.StartLogin();
        webViewOAuthService.StartLogin("kakao");
    }

    //private void LoginWithMockJson(string fileNameWithoutExtension)
    //{
    //    TextAsset jsonFile = Resources.Load<TextAsset>(fileNameWithoutExtension);

    //    if (jsonFile == null)
    //    {
    //        Debug.LogError($"Mock JSON file not found: {fileNameWithoutExtension}");
    //        return;
    //    }

    //    Debug.Log($"[AuthManager] Loaded mock file: {fileNameWithoutExtension}");

    //    OAuthLoginResponse response = JsonUtility.FromJson<OAuthLoginResponse>(jsonFile.text);

    //    if (response == null)
    //    {
    //        Debug.LogError("[AuthManager] Failed to parse mock JSON!!");
    //        return;
    //    }

    //    if (response.isSuccess) OnLoginSuccess(response);
    //    else OnLoginFail("Mock login failed......");
    //}

    private void HandleAuthorizationCode(string provider, string authorizationCode)
    {
        Debug.LogError($"[AuthManager] Authorization code received, provider={provider}");

        StartCoroutine(authApi.LoginWithOAuth(
            provider, authorizationCode, OnLoginSuccess, OnLoginFail
        ));
    }

    public void OnReceiveCodeFromJS(string codeData)
    {
        string[] parts = codeData.Split(':');
        if (parts.Length == 2)
        {
            string provider = parts[0];
            string code = parts[1];

            Debug.Log($"[AuthManager] WebGL Code Received: {provider}");

            HandleAuthorizationCode(provider, code);
        }
    }

    private void OnLoginSuccess(OAuthLoginResponse response)
    {
        if (response == null || response.data == null)
        {
            Debug.LogError("[AuthManager] Invalid login response.");
            return;
        }

        Debug.Log("[AuthManager] OnLoginSuccess called");
        //Debug.Log($"[AuthManager] access tokne = {response.data.accessToken}");
        //Debug.Log($"[AuthManager] refresh token = {response.data.refreshToken}.");

        TokenStore.Instance.SetLoginData(response.data);

        //Debug.Log($"[AuthManager] tokenStore.HasAccessToken() = {TokenStore.Instance.HasAccessToken()}");
        //Debug.Log($"[AuthManager] Login success: {response.data.nickname}, provider={response.data.provider}, isNewUser={response.data.isNewUser}");

        if (response.data.isNewUser) SceneManager.LoadScene("Lobby");
        else SceneManager.LoadScene("Lobby");
    }

    private void OnLoginFail(string errorMessage)
    {
        Debug.LogError($"[AuthManager] Login failed: {errorMessage}");
    }
}

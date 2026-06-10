using UnityEngine;

public class TokenStore : MonoBehaviour
{
    public static TokenStore Instance { get; private set; }

    public string UserId { get; private set; }
    public string Email { get; private set; }
    public string Nickname { get; private set; }
    public string ProfileImageUrl { get; private set; }
    public string Provider { get; private set; }
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public bool IsNewUser { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLoginData(OAuthLoginData data)
    {
        if (data == null)
        {
            Debug.LogError("[TokenStore] SetLoginData failed: data is null.");
            return;
        }

        UserId = data.userId;
        Email = data.email;
        Nickname = data.nickname;
        ProfileImageUrl = data.profileImageUrl;
        Provider = data.provider;
        AccessToken = data.accessToken;
        RefreshToken = data.refreshToken;
        IsNewUser = data.isNewUser;

        Debug.Log($"[TokenStore] Login data saved. userId={UserId}, nickname={Nickname}, provider={Provider}");
    }

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void Clear()
    {
        UserId = null;
        Email = null;
        Nickname = null;
        ProfileImageUrl = null;
        Provider = null;
        AccessToken = null;
        RefreshToken = null;
        IsNewUser = false;
    }
}
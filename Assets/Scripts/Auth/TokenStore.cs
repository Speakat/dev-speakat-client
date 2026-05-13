using UnityEngine;

public class TokenStore : MonoBehaviour
{
    public static TokenStore Instance { get; private set; }

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string Email { get; private set; }
    public string Nickname { get; private set; }
    public string Provider { get; private set; }
    public string ProfileImageUrl { get; private set; }
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
        AccessToken = data.accessToken;
        RefreshToken = data.refreshToken;
        Email = data.email;
        Nickname = data.nickname;
        Provider = data.provider;
        ProfileImageUrl = data.profileImageUrl;
        IsNewUser = data.isNewUser;
    }

    public bool HasAccessToken()
    {
        return !string.IsNullOrEmpty(AccessToken);
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        Email = null;
        Nickname = null;
        Provider = null;
        ProfileImageUrl = null;
        IsNewUser = false;
    }
}

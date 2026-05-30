using UnityEngine;

public class TokenStore : MonoBehaviour
{
    public static TokenStore Instance { get; private set; }

    public string UserUuid { get; private set; }
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string Email { get; private set; }
    public string Nickname { get; private set; }
    public int Provider { get; private set; }
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
        if (data == null)
        {
            Debug.LogError("[TokenStore] Login data is null");
            return;
        }

        UserUuid = data.userUuid;
        AccessToken = data.accessToken;
        RefreshToken = data.refreshToken;
        Email = data.email;
        Nickname = data.nickname;
        Provider = data.provider;
        ProfileImageUrl = data.profileImageUrl;
        IsNewUser = data.isNewUser;

        Debug.Log($"[TokenStore] Login data saved: uuid={UserUuid}, nickname={Nickname}, provider={Provider}, isNewUser={IsNewUser}");
    }

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public bool HasAccessToken()
    {
        return !string.IsNullOrEmpty(AccessToken);
    }

    public void Clear()
    {
        UserUuid = null;
        AccessToken = null;
        RefreshToken = null;
        Email = null;
        Nickname = null;
        Provider = 0;
        ProfileImageUrl = null;
        IsNewUser = false;
    }
}
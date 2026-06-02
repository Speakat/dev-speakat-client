using UnityEngine;

public class LobbyTokenDebug : MonoBehaviour
{
    private void Start()
    {
        if (TokenStore.Instance == null)
        {
            Debug.LogError("[LobbyTokenDebug] TokenStore.Instance is null");
            return;
        }

        TokenStore tokenStore = TokenStore.Instance;

        Debug.Log($"[LobbyTokenDebug] UserId: {tokenStore.UserId}");
        Debug.Log($"[LobbyTokenDebug] Email: {tokenStore.Email}");
        Debug.Log($"[LobbyTokenDebug] Nickname: {tokenStore.Nickname}");
        Debug.Log($"[LobbyTokenDebug] Provider: {tokenStore.Provider}");
        Debug.Log($"[LobbyTokenDebug] HasAccessToken: {!string.IsNullOrWhiteSpace(tokenStore.AccessToken)}");
        Debug.Log($"[LobbyTokenDebug] HasRefreshToken: {!string.IsNullOrWhiteSpace(tokenStore.RefreshToken)}");
    }
}
using UnityEngine;

public class LobbyTokenDebug : MonoBehaviour
{
    private void Start()
    {
        if (TokenStore.Instance == null)
        {
            Debug.LogError("[Lobby] TokenStore.Instance is NULL!");
            return;
        }

        Debug.Log($"[Lobby] TokenStore object: {TokenStore.Instance.gameObject.name}");
        Debug.Log($"[Lobby] userUuid: {TokenStore.Instance.UserUuid}");
        Debug.Log($"[Lobby] accessToken: {TokenStore.Instance.AccessToken}");
        Debug.Log($"[Lobby] refreshToken: {TokenStore.Instance.RefreshToken}");

        Debug.Log($"[Lobby] User: {TokenStore.Instance.Nickname} ({TokenStore.Instance.Email})");
        Debug.Log($"[Lobby] provider: {TokenStore.Instance.Provider}");

        if (TokenStore.Instance.HasAccessToken())
            Debug.Log("[Lobby] Login session exists");
        else
            Debug.LogError("[Lobby] No access token,,");
    }
}

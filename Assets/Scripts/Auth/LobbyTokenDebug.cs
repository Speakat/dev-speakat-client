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

        //Debug.Log($"[Lobby] TokenStore object: {TokenStore.Instance.gameObject.name}");
        //Debug.Log($"[Lobby] accessToken: {TokenStore.Instance.GetAccessToken()}");
        //Debug.Log($"[Lobby] refreshToken: {TokenStore.Instance.GetRefreshToken()}");

        if (TokenStore.Instance.HasAccessToken()) Debug.Log("[Lobby] Login session exists");
        else Debug.LogError("[Lobby] No access token,,");
    }
}

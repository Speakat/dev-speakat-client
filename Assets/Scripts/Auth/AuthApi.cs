using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthApi : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://api-domain.com"; //수정할예정

    public IEnumerator LoginWithOAuth(
        string provider,
        string authorizationCode,
        Action<OAuthLoginResponse> onSuccess,
        Action<string> onFail)
    {
        string url = $"{baseUrl}/auth/oauth/{provider}";

        OAuthLoginRequest requestBody = new OAuthLoginRequest
        {
            authorizationCode = authorizationCode
        };

        string json = JsonUtility.ToJson(requestBody);

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"[AuthApi] POST {url}");
        Debug.Log($"[AuthApi] Body: {json}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[AuthApi] Response: {req.downloadHandler.text}");
            OAuthLoginResponse response = JsonUtility.FromJson<OAuthLoginResponse>(req.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            Debug.LogError($"[AuthApi] Error: {req.error}\n{req.downloadHandler.text}");
            onFail?.Invoke(req.downloadHandler.text);
        }
    }
}

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthApi : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://speakat.hyorim.shop";

    private string BuildUrl(string path)
    {
        string root = baseUrl.TrimEnd('/');
        string p = path.StartsWith("/") ? path : "/" + path;
        return root + p;
    }

    public IEnumerator LoginWithOAuth(
        string provider,
        string authorizationCode,
        Action<OAuthLoginResponse> onSuccess,
        Action<string> onFail)
    {
        string url = BuildUrl($"/auth/oauth/{provider}");

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
        req.SetRequestHeader("Accept", "application/json");

        Debug.Log($"[AuthApi] POST {url}");
        Debug.Log($"[AuthApi] Body: {json}");

        yield return req.SendWebRequest();

        string responseText = req.downloadHandler != null ? req.downloadHandler.text : "";

        Debug.Log($"[AuthApi] Status: {req.responseCode}");
        Debug.Log($"[AuthApi] Response: {responseText}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            string error = $"HTTP {req.responseCode}: {req.error}\n{responseText}";
            Debug.LogError($"[AuthApi] Login Error: {error}");
            onFail?.Invoke(error);
            yield break;
        }

        OAuthLoginResponse response = null;

        try
        {
            response = JsonUtility.FromJson<OAuthLoginResponse>(responseText);
        }
        catch (Exception e)
        {
            string error = $"Login response parse failed: {e.Message}\n{responseText}";
            Debug.LogError($"[AuthApi] {error}");
            onFail?.Invoke(error);
            yield break;
        }

        if (response == null)
        {
            onFail?.Invoke("[AuthApi] Login response is null");
            yield break;
        }

        if (!response.isSuccess)
        {
            string message = string.IsNullOrEmpty(response.message)
                ? "Login failed"
                : response.message;

            Debug.LogError($"[AuthApi] API Fail: code={response.code}, message={message}");
            onFail?.Invoke(message);
            yield break;
        }

        if (response.data == null)
        {
            onFail?.Invoke("[AuthApi] Login success but data is null");
            yield break;
        }

        onSuccess?.Invoke(response);
    }

    public IEnumerator RefreshToken(
        string refreshToken,
        Action<RefreshTokenResponse> onSuccess,
        Action<string> onFail)
    {
        string url = BuildUrl("/auth/refresh");

        RefreshTokenRequest requestBody = new RefreshTokenRequest
        {
            refreshToken = refreshToken
        };

        string json = JsonUtility.ToJson(requestBody);

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "application/json");

        Debug.Log($"[AuthApi] POST {url}");
        Debug.Log($"[AuthApi] Body: {json}");

        yield return req.SendWebRequest();

        string responseText = req.downloadHandler != null ? req.downloadHandler.text : "";

        if (req.result != UnityWebRequest.Result.Success)
        {
            string error = $"HTTP {req.responseCode}: {req.error}\n{responseText}";
            Debug.LogError($"[AuthApi] Refresh Error: {error}");
            onFail?.Invoke(error);
            yield break;
        }

        RefreshTokenResponse response = JsonUtility.FromJson<RefreshTokenResponse>(responseText);

        if (response == null || !response.isSuccess || response.data == null)
        {
            string error = response != null ? response.message : "Refresh response is null";
            Debug.LogError($"[AuthApi] Refresh API Fail: {error}");
            onFail?.Invoke(error);
            yield break;
        }

        onSuccess?.Invoke(response);
    }
}
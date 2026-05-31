using System;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;
using Speakat.Client;

public class SpeakatApiProvider : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://speakat.hyorim.shop/";
    [SerializeField] private string debugAccessToken;

    private HttpClient httpClient;
    private SpeakatClient client;

    public SpeakatClient Client
    {
        get
        {
            if (client == null)
            {
                CreateClient();
            }

            return client;
        }
    }

    private void Awake()
    {
        CreateClient();
    }

    private void CreateClient()
    {
        httpClient?.Dispose();

        string normalizedBaseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";

        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(normalizedBaseUrl);

        string accessToken = ResolveAccessToken();

        if (!string.IsNullOrEmpty(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            Debug.Log("[SpeakatApiProvider] Authorization Bearer token 적용 완료");
            Debug.Log($"[SpeakatApiProvider] Authorization Scheme={httpClient.DefaultRequestHeaders.Authorization?.Scheme}");
            Debug.Log($"[SpeakatApiProvider] Authorization Parameter Exists={!string.IsNullOrEmpty(httpClient.DefaultRequestHeaders.Authorization?.Parameter)}");
        }
        else
        {
            Debug.LogWarning("[SpeakatApiProvider] AccessToken이 없습니다. 비로그인 상태로 API Client를 생성합니다.");
        }

        client = new SpeakatClient(httpClient);
        client.BaseUrl = normalizedBaseUrl;

        Debug.Log($"[SpeakatApiProvider] BaseUrl={client.BaseUrl}");
        Debug.Log($"[SpeakatApiProvider] HttpClient.BaseAddress={httpClient.BaseAddress}");
    }

    private string ResolveAccessToken()
    {
        string accessToken = null;

        if (TokenStore.Instance != null && !string.IsNullOrEmpty(TokenStore.Instance.AccessToken))
        {
            accessToken = TokenStore.Instance.AccessToken.Trim();
            Debug.Log("[SpeakatApiProvider] TokenStore.AccessToken을 사용합니다.");
        }
        else if (!string.IsNullOrEmpty(debugAccessToken))
        {
            accessToken = debugAccessToken.Trim();
            Debug.LogWarning("[SpeakatApiProvider] TokenStore가 없어 debugAccessToken을 사용합니다.");
        }

        if (!string.IsNullOrEmpty(accessToken))
        {
            Debug.Log($"[SpeakatApiProvider] token length={accessToken.Length}");
            Debug.Log($"[SpeakatApiProvider] token startsWith eyJ={accessToken.StartsWith("eyJ")}");
        }

        return accessToken;
    }

    public void RefreshAuthHeader()
    {
        if (httpClient == null)
        {
            CreateClient();
            return;
        }

        string accessToken = ResolveAccessToken();

        httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            Debug.Log("[SpeakatApiProvider] Authorization 헤더 갱신 완료");
        }
    }

    private void OnDestroy()
    {
        httpClient?.Dispose();
    }
}
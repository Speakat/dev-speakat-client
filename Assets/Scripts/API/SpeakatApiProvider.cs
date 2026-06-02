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

            RefreshAuthHeader();
            return client;
        }
    }

    private void Awake()
    {
        CreateClient();
    }

    private void CreateClient()
    {
        if (httpClient != null && client != null)
        {
            return;
        }

        string normalizedBaseUrl = NormalizeBaseUrl(baseUrl);

        httpClient = new HttpClient
        {
            BaseAddress = new Uri(normalizedBaseUrl)
        };

        client = new SpeakatClient(httpClient)
        {
            BaseUrl = normalizedBaseUrl
        };

        RefreshAuthHeader();

        Debug.Log($"[SpeakatApiProvider] BaseUrl={client.BaseUrl}");
        Debug.Log($"[SpeakatApiProvider] HttpClient.BaseAddress={httpClient.BaseAddress}");
    }

    private string NormalizeBaseUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "https://speakat.hyorim.shop/";
        }

        return url.EndsWith("/") ? url : url + "/";
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
            Debug.LogWarning("[SpeakatApiProvider] TokenStore가 비어 있어 debugAccessToken을 사용합니다.");
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
            return;
        }

        string accessToken = ResolveAccessToken();

        httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            Debug.Log("[SpeakatApiProvider] Authorization Bearer token 설정 완료");
        }
        else
        {
            Debug.LogWarning("[SpeakatApiProvider] AccessToken이 없습니다. 비로그인 상태로 API Client를 사용합니다.");
        }
    }

    private void OnDestroy()
    {
        httpClient?.Dispose();
        httpClient = null;
        client = null;
    }
}
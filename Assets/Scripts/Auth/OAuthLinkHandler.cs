using System;
using UnityEngine;

public class OAuthLinkHandler : MonoBehaviour
{
    public event Action<string, string> OnAuthorizationCodeReceived;

    private void Awake()
    {
        Application.deepLinkActivated += HandleDeepLink;

        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            HandleDeepLink(Application.absoluteURL);
        }
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= HandleDeepLink;
    }

    private void HandleDeepLink(string url)
    {
        Debug.Log($"Deep link received: {url}");

        Uri uri = new Uri(url);
        string code = GetQueryParam(uri.Query, "code");

        string provider = ExtractProvider(uri);

        if (!string.IsNullOrEmpty(provider) && !string.IsNullOrEmpty(code))
        {
            OnAuthorizationCodeReceived?.Invoke(provider, code);
        }
    }

    private string ExtractProvider(Uri uri)
    {
        string full = uri.AbsoluteUri.ToLower();

        if (full.Contains("/google") || full.Contains("google"))
            return "google";
        if (full.Contains("/kakao") || full.Contains("kakao"))
            return "kakao";

        return null;
    }

    private string GetQueryParam(string query, string key)
    {
        if (string.IsNullOrEmpty(query)) return null;
        if (query.StartsWith("?")) query = query.Substring(1);

        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach ( var pair in pairs)
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0] == key)
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }
}

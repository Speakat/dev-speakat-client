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
        Debug.Log($"[OAuthLinkHandler] Deep link received: {url}");

        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("[OAuthLinkHandler] Empty deep link url.");
            return;
        }

        Uri uri;

        try
        {
            uri = new Uri(url);
        }
        catch (Exception e)
        {
            Debug.LogError($"[OAuthLinkHandler] Invalid deep link url: {url}, error={e.Message}");
            return;
        }

        string error = GetQueryParam(uri.Query, "error");

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"[OAuthLinkHandler] OAuth error from deep link: {error}");
            return;
        }

        string code = GetQueryParam(uri.Query, "code");
        string provider = ExtractProvider(uri);

        Debug.Log($"[OAuthLinkHandler] provider={provider}, hasCode={!string.IsNullOrEmpty(code)}");

        if (!string.IsNullOrEmpty(provider) && !string.IsNullOrEmpty(code))
        {
            OnAuthorizationCodeReceived?.Invoke(provider, code);
        }
        else
        {
            Debug.LogWarning($"[OAuthLinkHandler] provider/code missing. provider={provider}, codeEmpty={string.IsNullOrEmpty(code)}");
        }
    }

    private string ExtractProvider(Uri uri)
    {
        // speakat://oauth/google?code=xxx
        string path = uri.AbsolutePath.Trim('/').ToLower();
        string[] segments = path.Split('/');
        string lastSegment = segments[segments.Length - 1];

        if (lastSegment == "google") return "google";
        if (lastSegment == "kakao") return "kakao";

        // host 기반 fallback: speakat://google?code=xxx 형태
        string host = uri.Host.ToLower();

        if (host == "google") return "google";
        if (host == "kakao") return "kakao";

        Debug.LogWarning($"[OAuthLinkHandler] provider를 추출할 수 없습니다. path={path}, host={host}");
        return null;
    }

    private string GetQueryParam(string query, string key)
    {
        if (string.IsNullOrEmpty(query)) return null;
        if (query.StartsWith("?")) query = query.Substring(1);

        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0] == key)
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }
}
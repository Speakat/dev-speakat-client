using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DebugFlashcardsRawTester : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://speakat.hyorim.shop";
    [SerializeField] private string debugAccessToken;

    private void Start()
    {
        StartCoroutine(TestFlashcardsRaw());
    }

    private IEnumerator TestFlashcardsRaw()
    {
        string url = baseUrl.TrimEnd('/') + "/flashcards?size=20";

        using UnityWebRequest req = UnityWebRequest.Get(url);

        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(debugAccessToken))
        {
            string token = debugAccessToken.Trim();
            req.SetRequestHeader("Authorization", "Bearer " + token);

            Debug.Log($"[RawTester] token length={token.Length}");
            Debug.Log($"[RawTester] token startsWith eyJ={token.StartsWith("eyJ")}");
        }
        else
        {
            Debug.LogWarning("[RawTester] debugAccessToken이 비어 있습니다.");
        }

        Debug.Log($"[RawTester] GET {url}");

        yield return req.SendWebRequest();

        Debug.Log($"[RawTester] result={req.result}");
        Debug.Log($"[RawTester] status={req.responseCode}");
        Debug.Log($"[RawTester] response={req.downloadHandler.text}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[RawTester] error={req.error}");
        }
    }
}
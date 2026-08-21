using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FeedbackPopupUIController : MonoBehaviour
{
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private Button replayButton;
    [SerializeField]
    private TextMeshProUGUI feedbackText;
    [SerializeField]
    private TextMeshProUGUI wordText;

    public Image saveIcon;

    public Sprite defaultSaveIcon;

    public Sprite successSaveIcon;

    //private const string BaseUrl = "https://speakat.hyorim.shop";
    [SerializeField] private SpeakatApiProvider apiProvider;

    private const string FlashcardsEndpoint = "/flashcards";
    private string currentFeedback = "";
    private List<string> currentSuggestions = new List<string>();

    private string BuildUrl(string endpoint)
    {
        if (apiProvider == null)
            throw new System.Exception("[FeedbackPopupUIController] apiProvider가 연결되지 않았습니다.");

        string path = endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
        return apiProvider.ApiBaseUrl.TrimEnd('/') + path;
    }

    private void Awake()
    {
        replayButton.onClick.AddListener(ReplayDialogue);
        saveButton.onClick.AddListener(SaveWord);
    }

    public void SetFeedbackPopup(string feedback, List<string> suggestions)
    {
        currentFeedback = feedback;
        currentSuggestions = suggestions;

        saveIcon.sprite = defaultSaveIcon;
        saveButton.interactable = true;

        SetFeedback(feedback);
        SetWord(string.Join(", ", suggestions));

        gameObject.SetActive(true);
    }

    private void SetFeedback(string feedback)
    {
        feedbackText.text = feedback;
    }

    private void SetWord(string word)
    {
        wordText.text = word;
    }

    private void ReplayDialogue()
    {
        gameObject.SetActive(false);
    }

    private void SaveWord()
    {
        Debug.Log($"[FeedbackPopup] 저장 버튼 클릭: feedback='{currentFeedback}', suggestions=[{string.Join(", ", currentSuggestions)}]");
        if (currentSuggestions == null || currentSuggestions.Count == 0)
        {
            Debug.LogWarning("[FeedbackPopup] 저장할 단어가 없습니다.");
            return;
        }

        StartCoroutine(PostAllFlashcardsCoroutine());
    }

    private IEnumerator PostAllFlashcardsCoroutine()
    {
        saveButton.interactable = false;

        int questId = SceneContext.SelectedQuestId != 0 ? SceneContext.SelectedQuestId : 1;

        foreach (string word in currentSuggestions)
        {
            string body = JsonUtility.ToJson(new FlashcardRequest
            {
                questId = questId,
                word = word,
                recommendationReason = currentFeedback
            });

            bool success = false;
            string resultMessage = "";

            yield return StartCoroutine(PostCoroutine(BuildUrl(FlashcardsEndpoint), body,
                onSuccess: (response) => { success = true; resultMessage = response; },
                onFailure: (error) => { success = false; resultMessage = error; }
            ));

            if (success)
                Debug.Log($"[FeedbackPopup] 저장 성공: word={word}, response={resultMessage}");
            else
                Debug.Log($"[FeedbackPopup] 저장 실패: word={word}, error={resultMessage}");
        }

        saveButton.interactable = true;
        saveIcon.sprite = successSaveIcon; // 실패해도 항상 실행
    }
    private IEnumerator PostCoroutine(string url, string bodyJson, System.Action<string> onSuccess, System.Action<string> onFailure)
    {
        byte[] bodyBytes = Encoding.UTF8.GetBytes(bodyJson);
        string token = TokenStore.Instance.AccessToken.Trim();

        using UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(bodyBytes);
        req.uploadHandler.contentType = "application/json";
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onFailure?.Invoke($"[{req.responseCode}] {req.error} — {req.downloadHandler.text}");
    }
}
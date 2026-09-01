using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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

    [SerializeField] private FlashcardApiService flashcardApiService;

    public Image saveIcon;

    public Sprite defaultSaveIcon;

    public Sprite successSaveIcon;

    private string currentFeedback = "";
    private List<string> currentSuggestions = new List<string>();

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

    private async void SaveWord()
    {
        if (currentSuggestions == null || currentSuggestions.Count == 0)
        {
            Debug.LogWarning("[FeedbackPopup] 저장할 단어가 없습니다.");
            return;
        }

        await SaveAllFlashcardsAsync();
    }

    private async Task SaveAllFlashcardsAsync()
    {
        if (flashcardApiService == null)
        {
            Debug.LogError("[FeedbackPopup] flashcardApiService is not assigned.");
            return;
        }

        saveButton.interactable = false;

        int questId = SceneContext.SelectedQuestId != 0 ? SceneContext.SelectedQuestId : 1;
        List<string> failedWords = new List<string>();

        foreach (string word in currentSuggestions)
        {
            try
            {
                await flashcardApiService.SaveAsync(questId, word, currentFeedback);
            }
            catch (System.Exception exception)
            {
                failedWords.Add(word);
                Debug.LogError($"[FeedbackPopup] 단어 저장 실패: {ApiErrorMessage.From(exception)}");
            }
        }

        currentSuggestions = failedWords;
        bool allSucceeded = failedWords.Count == 0;
        saveButton.interactable = !allSucceeded;
        saveIcon.sprite = allSucceeded ? successSaveIcon : defaultSaveIcon;

        if (!allSucceeded)
        {
            SetWord(string.Join(", ", failedWords));
        }
    }
}

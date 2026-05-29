using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField]
    private TextMeshProUGUI meanText;
    
    private void Awake()
    {
        replayButton.onClick.AddListener(ReplayDialogue);
        saveButton.onClick.AddListener(SaveWord);
    }

    public void SetFeedbackPopup(string feedback, List<string> suggestions)
    {
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

    // 해당 대화 재시작
    private void ReplayDialogue()
    {
        gameObject.SetActive(false);
    }

    // 단어 저장
    private void SaveWord()
    {
        gameObject.SetActive(false);
    }
}

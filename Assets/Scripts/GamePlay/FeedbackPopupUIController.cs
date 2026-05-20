using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackPopupUIController : MonoBehaviour
{
    [SerializeField]
    private Button replayButton;
    [SerializeField]
    private Button continueButton;
    [SerializeField]
    private TextMeshProUGUI feedbackText;
    [SerializeField]
    private TextMeshProUGUI wordText;
    
    private void Awake()
    {
        replayButton.onClick.AddListener(ReplayDialogue);
        continueButton.onClick.AddListener(ContinueDialogue);
    }

    public void SetFeedback(string feedback)
    {
        feedbackText.text = feedback;
    }

    public void SetWord(string word)
    {
        wordText.text = word;
    }

    // 해당 대화 재시작
    private void ReplayDialogue()
    {
        Debug.Log("대화를 재시작합니다.");
        gameObject.SetActive(false);
    }

    // 다음 대화로 이동
    private void ContinueDialogue()
    {
        gameObject.SetActive(false);
    }
}

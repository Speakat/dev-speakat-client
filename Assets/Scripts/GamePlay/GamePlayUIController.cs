using UnityEngine;
using UnityEngine.UI;

public class GamePlayUIController : MonoBehaviour
{
    [SerializeField]
    private DialoguePanelUIController dialoguePanel;
    [SerializeField]
    private LogPanelUIController logPanel;
    [SerializeField]
    private Button logButton;
    [SerializeField]
    private FeedbackPopupUIController feedbackPopup;

    private void Awake()
    {
        logButton.onClick.AddListener(() => logPanel.gameObject.SetActive(true));
    }

    private void Start()
    {
        dialoguePanel.SetQuestionPanel("What do you want to do?");
        dialoguePanel.SetAnswerPanel("I want to go to the market.");

        logPanel.AddLog(LogType.Question, "What do you want to do?");
        logPanel.AddLog(LogType.Answer, "I want to go to the market.");
        logPanel.AddLog(LogType.Question, "Good!");
        logPanel.AddLog(LogType.Question, "What do you want to eat?");
        logPanel.AddLog(LogType.Answer, "I want to eat pizza.");
    }
}

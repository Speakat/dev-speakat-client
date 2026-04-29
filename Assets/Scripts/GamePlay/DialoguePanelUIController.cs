using TMPro;
using UnityEngine;

public class DialoguePanelUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject questionPanel;
    [SerializeField]
    private GameObject answerPanel;

    void Start()
    {
        SetQuestionPanel("What do you want to do?");
        SetAnswerPanel("I want to go to the market.");
    }

    public void SetQuestionPanel(string question)
    {
        questionPanel.SetActive(true);
        questionPanel.GetComponentInChildren<TextMeshProUGUI>().text = question;
    }

    public void SetAnswerPanel(string answers)
    {
        answerPanel.SetActive(true);
        answerPanel.GetComponentInChildren<TextMeshProUGUI>().text = answers;
    }
}

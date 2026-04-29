using TMPro;
using UnityEngine;

public class DialoguePanelUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject questionPanel;
    [SerializeField]
    private GameObject answerPanel;
    
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

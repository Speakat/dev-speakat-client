using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanelUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject questionPanel;
    [SerializeField]
    private GameObject answerPanel;
    private RectTransform dialoguePanel;

    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private float maxWidth = 300f;
    [SerializeField] private float horizontalPadding = 50f; // 좌우 패딩 합산

    private RectTransform questionPanelRect;

    private void Awake()
    {
        dialoguePanel = GetComponent<RectTransform>();

        ClearPanels();
    }

    public void SetQuestionPanel(string question)
    {
        questionPanel.SetActive(true);
        questionPanel.GetComponentInChildren<TextMeshProUGUI>().text = question;

        StartCoroutine(SetDialoguePanelRefresh());
    }

    public void SetAnswerPanel(string answers)
    {
        answerPanel.SetActive(true);
        answerPanel.GetComponentInChildren<TextMeshProUGUI>().text = answers;

        StartCoroutine(SetDialoguePanelRefresh());
    }

    IEnumerator SetDialoguePanelRefresh()
    {
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dialoguePanel);
    }

    public void ClearPanels()
    {
        questionPanel.SetActive(false);
        answerPanel.SetActive(false);
    }

    // public void Resize()
    // {
    //     // 한 줄일 때 텍스트의 자연스러운 너비 계산
    //     float preferredWidth = questionText.preferredWidth + horizontalPadding;

    //     if (preferredWidth <= maxWidth)
    //     {
    //         // 텍스트가 짧으면 → 너비를 텍스트에 맞게 줄임, 한 줄
    //         questionPanelRect.sizeDelta = new Vector2(preferredWidth, questionPanelRect.sizeDelta.y);
    //         questionText.rectTransform.sizeDelta = new Vector2(preferredWidth - horizontalPadding, questionText.rectTransform.sizeDelta.y);
    //     }
    //     else
    //     {
    //         // 텍스트가 길면 → 최대 너비 고정, 여러 줄로 줄바꿈
    //         questionPanelRect.sizeDelta = new Vector2(maxWidth, questionPanelRect.sizeDelta.y);
    //         questionText.rectTransform.sizeDelta = new Vector2(maxWidth - horizontalPadding, questionText.rectTransform.sizeDelta.y);
    //     }
    // }
}

using UnityEngine;
using UnityEngine.UI;

public enum LogType
{
    Question,
    Answer
}

public class LogPanelUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject logListPanel;
    [SerializeField]
    private GameObject logQuestionPrefab;
    [SerializeField]
    private GameObject logAnswerPrefab;
    [SerializeField]
    private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    // log 하나씩 추가
    public void AddLog(LogType logType, string text)
    {
        GameObject logPrefab = logType == LogType.Question ? logQuestionPrefab : logAnswerPrefab;
        GameObject logItem = Instantiate(logPrefab, logListPanel.transform);
        logItem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;
    }

    public void ClearLogs()
    {
        foreach (Transform child in logListPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

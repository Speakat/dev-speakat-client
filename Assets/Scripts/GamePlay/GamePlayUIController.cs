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
}

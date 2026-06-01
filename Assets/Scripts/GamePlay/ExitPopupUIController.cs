using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitPopupUIController : MonoBehaviour
{
    [SerializeField]
    private Button exitButton;
    [SerializeField]
    private Button cancelButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        // TODO : 세션 종료 api 호출

        SceneManager.LoadScene("QuestScene");
    }

    private void OnCancelButtonClicked()
    {
        gameObject.SetActive(false);
    }
}

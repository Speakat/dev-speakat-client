using System.Threading.Tasks;
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
        exitButton.onClick.AddListener(() => _ = OnExitButtonClicked());
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private async Task OnExitButtonClicked()
    {
        await GamePlayManager.Instance.EndSessionAsync();
        SceneManager.LoadScene("QuestScene");
    }

    private void OnCancelButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI;
public class SessionExitButtonController : MonoBehaviour
{
    private Button exitButton;
    [SerializeField]
    private GameObject exitPopup;

    private void Awake()
    {
        exitButton = GetComponent<Button>();
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        exitPopup.SetActive(true);
    }
}

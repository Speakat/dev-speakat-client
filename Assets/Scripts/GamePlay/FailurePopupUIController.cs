using UnityEngine;
using UnityEngine.UI;

public class FailurePopupUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject failurePopup;

    [SerializeField]
    private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(ClosePopup);
    }

    private void ClosePopup()
    {
        failurePopup.SetActive(false);
    }

    public void ShowPopup()
    {
        failurePopup.SetActive(true);
    }
}

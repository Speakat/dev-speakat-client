using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FlashCardBackButton : MonoBehaviour
{
    [SerializeField] private FlashCardView flashCardView;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        Debug.Log("[FlashCardBackButton] 뒤로가기 버튼 클릭됨");

        if (flashCardView == null)
        {
            Debug.LogError("[FlashCardBackButton] flashCardView가 연결되지 않았습니다.");
            return;
        }

        flashCardView.Close();
    }
}
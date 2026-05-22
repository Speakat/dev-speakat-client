using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FilterTab : MonoBehaviour
{
    [SerializeField] private Image bgImage;     // 둥근 배경 이미지
    [SerializeField] private TMP_Text tabText;  // 필터 글자
    [SerializeField] private Button tabButton;

    public string FilterName {  get; private set; }

    public void Setup(string filterName, bool isSelected, System.Action<FilterTab> onClickAction)
    {
        FilterName = filterName;
        tabText.text = filterName;

        SetSelected(isSelected);

        tabButton.onClick.RemoveAllListeners();
        tabButton.onClick.AddListener(() => onClickAction(this));
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            bgImage.color = Color.black;
            tabText.color = Color.white;
        }
        else
        {
            bgImage.color = Color.white;
            tabText.color = Color.black;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FilterTab : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private TMP_Text tabText;
    [SerializeField] private Button tabButton;

    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite deselectedSprite;

    public string FilterName { get; private set; }

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
        // isSelected가 true(선택됨)면 검은색, false(안 선택됨)면 흰색
        bgImage.color = isSelected ? Color.black : Color.white;

        // 글자색은 반대로! 선택되면 흰색, 안 선택되면 검은색
        tabText.color = isSelected ? Color.white : Color.black;
    }
}
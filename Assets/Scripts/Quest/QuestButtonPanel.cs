using UnityEngine;

public class QuestButtonPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject questButton;

    public void SetButtonPosition(float dx)
    {
        questButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(dx, 0);
    }
}

using UnityEngine;

public class StageButtonPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject stageButton;

    public void SetButtonPosition(float dx)
    {
        stageButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(dx, 0);
    }
}

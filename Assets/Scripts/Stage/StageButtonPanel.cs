using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButtonPanel : MonoBehaviour
{
    [SerializeField]
    private RectTransform buttonRect;

    [SerializeField]
    private Button buttonComponent;
    public int StageId { get; set; }
    public string StageStatus { get; set; }

    public void SetStageButton(float dx, int id, string status)
    {
        StageId = id;
        StageStatus = status;

        // 버튼 위치 세팅
        buttonRect.anchoredPosition += new Vector2(dx, 0);

        // onClick 리스너 세팅
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(SelectStage);
    }

    public void SelectStage()
    {
        Debug.Log("Selected Stage: " + StageId + ", " + StageStatus);
        StageManager.Instance.SetSelectedStageId(StageId);
        SceneManager.LoadScene("QuestScene");
    }
}

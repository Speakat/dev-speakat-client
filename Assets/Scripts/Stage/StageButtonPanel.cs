using TMPro;
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

    [SerializeField]
    private TextMeshProUGUI titleText;

    public RectTransform lineImageTransform;
    private Image buttonImage;

    public Sprite completeImage; // 스테이지 완료를 나타내는 이미지
    public Sprite progressImage; // 스테이지 진행 중을 나타내는 이미지
    public Sprite lockedImage; // 스테이지 잠금을 나타내는 이미지

    private void Awake()
    {
        buttonImage = buttonComponent.GetComponent<Image>();
    }

    public void SetStageButton(float dx, int id, string status, string title)
    {
        float linedx = 93f;
        StageId = id;
        StageStatus = status;
        // 버튼 위치 세팅
        buttonRect.anchoredPosition += new Vector2(dx, 0);

        // onClick 리스너 세팅
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(SelectStage);

        // 제목 세팅
        titleText.text = title;

        // status에 따른 버튼 이미지 세팅
        if (status == "Completed")
        {
            buttonImage.sprite = completeImage;
        }
        else if (status == "Unlocked")
        {
            buttonImage.sprite = progressImage;
        }
        else if (status == "Locked")
        {
            buttonImage.sprite = lockedImage;
            buttonComponent.interactable = false; // 잠긴 스테이지는 클릭 불가능
        }

        if (dx < 0) // 왼쪽
        {
            lineImageTransform.anchoredPosition -= new Vector2(linedx, 0);
        }
        else // 오른쪽
        {
            lineImageTransform.anchoredPosition += new Vector2(linedx, 0);
            lineImageTransform.rotation = Quaternion.Euler(0, 180, 0); // 선 이미지 회전
        }
    }

    public void SelectStage()
    {
        Debug.Log("Selected Stage: " + StageId + ", " + StageStatus);
        SceneContext.SetSelectedStage(StageId);
        SceneManager.LoadScene("QuestScene");
    }

    private void LoadQuestScene()
    {
        SceneManager.LoadScene("QuestScene");
    }
}

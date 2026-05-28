using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestPopupUIController : MonoBehaviour
{
    private QuestDetailResponse questDetailResponse;
    private QuestDetailData questDetailData;

    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private TextMeshProUGUI[] objectivesText;
    [SerializeField]
    private Button startButton;

    private int currentQuestId;
    private int currentStageId;

    private string questDetailDummyData = @"
    {
    ""isSuccess"": true,
    ""data"": {
        ""questId"": 2,
        ""stageId"": 1,
        ""title"": ""커스텀 주문"",
        ""description"": ""자신만의 커스텀 음료를 주문해보세요."",
        ""thumbnailUrl"": ""https://cdn.speakat.com/quests/quest2-thumb.png"",
        ""objectives"": [
        ""음료 사이즈를 선택하세요"",
        ""커스텀 옵션을 2가지 이상 요청하세요"",
        ""최종 주문을 확인하세요""
        ],
        ""status"": ""IN_PROGRESS"",
        ""bestScore"": null,
        ""attemptCount"": 0
    }
    }";

    private void Awake()
    {
        startButton.onClick.AddListener(OnStart);

        questDetailResponse = JsonUtility.FromJson<QuestDetailResponse>(questDetailDummyData);
        questDetailData = questDetailResponse.data;
    }

    private void Start()
    {
        SetPopupUI(questDetailData);
    }

    public void SetPopupUI(QuestDetailData data)
    {
        currentQuestId = data.questId;
        currentStageId = data.stageId;

        titleText.text = data.title;
        descriptionText.text = data.description;

        for (int i = 0; i < objectivesText.Length; i++)
        {
            if (i < data.objectives.Count)
            {
                objectivesText[i].text = $"• {data.objectives[i]}";
                objectivesText[i].gameObject.SetActive(true);
            }
            else
            {
                objectivesText[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnStart()
    {
        SceneContext.SetSelectedStage(currentStageId);
        SceneContext.SetSelectedQuest(currentQuestId);

        SceneManager.LoadScene("GamePlayScene");
    }
}

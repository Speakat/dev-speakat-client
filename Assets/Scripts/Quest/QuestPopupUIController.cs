using System.Threading.Tasks;
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

    [SerializeField] private QuestApiService questApiService;

    public GameObject objectTextPrefab;
    public GameObject objectivesContainer;

    public GameObject questPopupPanel;
    public GameObject loadingPanel;


    private int currentQuestId;
    private int currentStageId;
    private int requestVersion;

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

        questPopupPanel.SetActive(false);
        loadingPanel.SetActive(true);
    }

    public async void SetQuestPopup(int questId)
    {
        currentQuestId = questId;
        int currentRequestVersion = ++requestVersion;
        await SetQuestPanel(currentRequestVersion);
    }

    public void SetPopupUI(QuestDetailData data)
    {
        currentQuestId = data.questId;
        currentStageId = data.stageId;

        titleText.text = data.title;
        descriptionText.text = data.description;

        foreach (Transform child in objectivesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < data.objectives.Count; i++)
        {
            GameObject objectiveGO = Instantiate(objectTextPrefab, objectivesContainer.transform);
            TextMeshProUGUI objectiveText = objectiveGO.GetComponent<TextMeshProUGUI>();
            objectiveText.text = $"• {data.objectives[i]}";
        }

        loadingPanel.SetActive(false);
        questPopupPanel.SetActive(true);
    }

    private void OnStart()
    {
        SceneContext.SetSelectedStage(currentStageId);
        SceneContext.SetSelectedQuest(currentQuestId);

        Destroy(QuestManager.Instance.gameObject);

        SceneManager.LoadScene("GamePlayScene");
    }

    private async Task SetQuestPanel(int currentRequestVersion)
    {
        try
        {
            if (questApiService == null)
            {
                throw new System.InvalidOperationException(
                    "[QuestPopupUIController] questApiService is not assigned.");
            }
            //Debug.Log($"[QuestPanelUIController] 요청 URL: {url}");
            //Debug.Log($"[QuestPanelUIController] 응답 raw: {json}");
            QuestDetailData data = await questApiService.GetQuestDetailAsync(currentQuestId);

            if (currentRequestVersion != requestVersion)
            {
                return;
            }

            SetPopupUI(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                $"[QuestPopupUIController] 퀘스트 상세 로드 실패 (questId={currentQuestId}): {ApiErrorMessage.From(e)}");
        }
    }

}

using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
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

    public GameObject objectTextPrefab;
    public GameObject objectivesContainer;

    public GameObject questPopupPanel;
    public GameObject loadingPanel;


    private const string BaseUrl = "http://speakat.hyorim.shop"; 
    private const string QuestDetailEndpoint = "/quests/{0}";

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

        questPopupPanel.SetActive(false);
        loadingPanel.SetActive(true);
    }

    public async void SetQuestPopup(int questId)
    {
        currentQuestId = questId;
        await SetQuestPanel();
    }

    public void SetPopupUI(QuestDetailData data)
    {
        currentQuestId = data.questId;
        currentStageId = data.stageId;

        titleText.text = data.title;
        descriptionText.text = data.description;

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

    public async Task SetQuestPanel()
    {
        try
        {
            string url = "https://speakat.hyorim.shop" + string.Format(QuestDetailEndpoint, currentQuestId);
            //Debug.Log($"[QuestPanelUIController] 요청 URL: {url}");
            string json = await GetAsync(url);
            //Debug.Log($"[QuestPanelUIController] 응답 raw: {json}");
            questDetailResponse = JsonUtility.FromJson<QuestDetailResponse>(json);
            SetPopupUI(questDetailResponse.data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestPanelUIController] 퀘스트 상세 로드 실패 (questId={currentQuestId}): {e.Message}");
        }
    }

    private async Task<string> GetAsync(string url)
    {
        string token = TokenStore.Instance.AccessToken.Trim();

        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            return req.downloadHandler.text;

        throw new System.Exception($"[{req.responseCode}] {req.error} — {req.downloadHandler.text}");
    }
}

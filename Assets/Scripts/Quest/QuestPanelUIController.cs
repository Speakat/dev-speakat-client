using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class QuestPanelUIController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI stageNameText;

    [SerializeField]
    private QuestButtonController[] questButtons;

    private StageDetailResponse stageDetailResponse;

    public GameObject questPanel;
    public GameObject loadingPanel;

    public int StageId { get; set; }


    private const string BaseUrl = "http://speakat.hyorim.shop";
    private const string StageDetailEndpoint = "/stages/";

    private string stageDetailDummyData = @"
{
  ""isSuccess"": true,
  ""data"": {
    ""stageId"": 1,
    ""title"": ""카페에서 주문하기"",
    ""description"": ""카페에서 음료를 주문하는 상황을 연습합니다."",
    ""status"": ""IN_PROGRESS"",
    ""quests"": [
      {
        ""questId"": 1,
        ""title"": ""인사하고 메뉴판 받기"",
        ""description"": ""점원에게 인사하고 메뉴판을 요청하세요."",
        ""thumbnailUrl"": ""https://cdn.speakat.com/quests/quest1-thumb.png"",
        ""sortOrder"": 1,
        ""status"": ""COMPLETED"",
        ""attemptCount"": 2
      },
      {
        ""questId"": 2,
        ""title"": ""음료 주문하기"",
        ""description"": ""원하는 음료와 수량을 말하세요."",
        ""thumbnailUrl"": ""https://cdn.speakat.com/quests/quest2-thumb.png"",
        ""sortOrder"": 2,
        ""status"": ""IN_PROGRESS"",
        ""attemptCount"": 1
      },
      {
        ""questId"": 3,
        ""title"": ""결제 및 인사"",
        ""description"": ""결제 수단을 선택하고 작별 인사를 하세요."",
        ""thumbnailUrl"": ""https://cdn.speakat.com/quests/quest3-thumb.png"",
        ""sortOrder"": 3,
        ""status"": ""LOCKED"",
        ""attemptCount"": 0
      }
    ]
  }
}";

    private void Awake()
    {
        loadingPanel.SetActive(true);
        questPanel.SetActive(false);
    }

    public void SetStageName(string stageName)
    {
        stageNameText.text = stageName;
    }

    public void SetQuestPanel()
    {
        _ = SetQuestPanelAsync();
    }

    public async Task SetQuestPanelAsync()
    {
        try
        {
            string url = BaseUrl + StageDetailEndpoint + StageId;
            //Debug.Log($"[QuestPanelUIController] 요청 URL: {url}");
            string json = await GetAsync(url);
            //Debug.Log($"[QuestPanelUIController] 응답 raw: {json}");
            stageDetailResponse = JsonUtility.FromJson<StageDetailResponse>(json);
            ApplyStageDetail(stageDetailResponse);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestPanelUIController] 스테이지 상세 로드 실패 (stageId={StageId}): {e.Message}");
        }
    }

    private void ApplyStageDetail(StageDetailResponse response)
    {   
        loadingPanel.SetActive(false);
        questPanel.SetActive(true);
        SetStageName(response.data.title);

        int completedQuestCount = SceneContext.GetCompletedQuestCount(StageId);

        for (int i = 0; i < questButtons.Length; i++)
        {
            if (i < response.data.quests.Count)
            {
                Debug.Log(response.data.quests[i].title + " - " + response.data.quests[i].isCompleted);
                QuestItem quest = response.data.quests[i];
                questButtons[i].gameObject.SetActive(true);
                questButtons[i].SetQuestButton(quest.questId, quest.isCompleted);
            }
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
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
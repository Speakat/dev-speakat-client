using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private StageScrollUIController stageScrollUIController;

    //private const string BaseUrl = "https://speakat.hyorim.shop";
    [SerializeField] private SpeakatApiProvider apiProvider;

    private const string StageListEndpoint = "/stages";

    private string BuildUrl(string endpoint)
    {
        if (apiProvider == null)
            throw new System.Exception("[StageManager] apiProvider가 연결되지 않았습니다.");

        string path = endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
        return apiProvider.ApiBaseUrl.TrimEnd('/') + path;
    }

    private string stageListDummyData = @"
    {
      ""isSuccess"": true,
      ""data"": {
        ""items"": [
          {
            ""stageId"": 1,
            ""title"": ""카페에서 주문하기"",
            ""description"": ""카페에서 음료를 주문하는 상황을 연습합니다."",
            ""thumbnailUrl"": ""https://cdn.speakat.com/stages/1/thumb.png"",
            ""status"": ""COMPLETED"",
            ""questCount"": 3,
            ""completedQuestCount"": 3
          },
          {
            ""stageId"": 2,
            ""title"": ""공항 체크인"",
            ""description"": ""공항에서 체크인하는 상황을 연습합니다."",
            ""thumbnailUrl"": ""https://cdn.speakat.com/stages/2/thumb.png"",
            ""status"": ""UNLOCKED"",
            ""questCount"": 4,
            ""completedQuestCount"": 1
          },
          {
            ""stageId"": 3,
            ""title"": ""비즈니스 미팅"",
            ""description"": ""비즈니스 미팅에서 의견을 교환하는 상황입니다."",
            ""thumbnailUrl"": ""https://cdn.speakat.com/stages/3/thumb.png"",
            ""status"": ""LOCKED"",
            ""questCount"": 5,
            ""completedQuestCount"": 0
          }
        ]
      }
    }";
    async void Start()
    {
        await RefreshStageList();
    }

    public async Task RefreshStageList()
    {
        await LoadStageListAsync();
    }

    private async Task LoadStageListAsync()
    {
        try
        {
            string json = await GetAsync(BuildUrl(StageListEndpoint));
            StageData stageListData = JsonUtility.FromJson<StageData>(json);
            SceneContext.SetStageListData(stageListData.data);
            stageScrollUIController.SetStageScrollUI(stageListData.data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StageManager] 스테이지 목록 로드 실패: {e.Message}");
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
        {
            //Debug.Log($"[StageManager] GET {url} 성공: {req.downloadHandler.text}");
            return req.downloadHandler.text;
        }

        throw new System.Exception($"[{req.responseCode}] {req.error} — {req.downloadHandler.text}");
    }
}
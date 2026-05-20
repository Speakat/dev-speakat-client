using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    private StageScrollUIController stageScrollUIController;

    public static StageManager Instance { get; private set; }

    private StageData stageListData;
    public StageData StageListData => stageListData;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        stageListData = JsonUtility.FromJson<StageData>(stageListDummyData);
        stageScrollUIController.SetStageScrollUI(stageListData.data);
    }

    public int GetStageCount()
    {
        return stageListData.data.items.Count;
    }
}
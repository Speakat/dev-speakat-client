using TMPro;
using UnityEngine;

public class QuestPanelUIController : MonoBehaviour
{
  [SerializeField]
  private TextMeshProUGUI stageNameText; // 스테이지 이름 텍스트

  [SerializeField]
  private QuestButtonController[] questButtons;

  private StageDetailResponse stageDetailResponse;

  // 더미데이터
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
  public void SetStageName(string stageName)
  {
    stageNameText.text = stageName;
  }

  // TODO: API 연동 시 더미 파싱을 FetchStageDetail(stageId) 호출로 교체
  //       슬라이드 전환 시 인접 스테이지 선호출 + 캐싱 구조 함께 적용 예정
  public void SetQuestPanel(int stageId = 1)
  {
    stageDetailResponse = JsonUtility.FromJson<StageDetailResponse>(stageDetailDummyData);

    SetStageName(stageDetailResponse.data.title);

    for (int i = 0; i < questButtons.Length; i++)
    {
      if (i < stageDetailResponse.data.quests.Count)
      {
        QuestItem quest = stageDetailResponse.data.quests[i];

        // 데이터가 있으면 버튼 활성화
        questButtons[i].gameObject.SetActive(true);

        // 버튼 컨트롤러에 데이터 전달
        questButtons[i].SetQuestButton(quest.questId, quest.status);
      }
      else
      {
        questButtons[i].gameObject.SetActive(false);
      }
    }
  }
}

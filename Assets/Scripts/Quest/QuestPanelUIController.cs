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
        ""status"": ""UNLOCKED"",
        ""quests"": [
          {
            ""questId"": 1,
            ""title"": ""인사하고 메뉴판 받기"",
            ""description"": ""점원에게 인사하고 메뉴판을 요청하세요."",
            ""sortOrder"": 1,
            ""isCompleted"": true,
            ""attemptCount"": 2
          },
          {
            ""questId"": 2,
            ""title"": ""음료 주문하기"",
            ""description"": ""원하는 음료와 수량을 말하세요."",
            ""sortOrder"": 2,
            ""isCompleted"": false,
            ""attemptCount"": 1
          },
          {
            ""questId"": 3,
            ""title"": ""결제 및 인사"",
            ""description"": ""결제 수단을 선택하고 작별 인사를 하세요."",
            ""sortOrder"": 3,
            ""isCompleted"": false,
            ""attemptCount"": 0
          }
        ]
      },
      ""code"": null,
      ""message"": null
    }";

    public void SetStageName(string stageName)
    {
        stageNameText.text = stageName;
    }

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
                questButtons[i].SetQuestButton(quest.questId, quest.isCompleted);
            }
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
        }
    }
}

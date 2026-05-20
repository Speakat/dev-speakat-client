using UnityEngine;

public class StageScrollUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject stageButtonPrefab;

    public void SetStageScrollUI(StageList stageList)
    {
        for (int i = 0; i < stageList.items.Count; i++)
        {
            GameObject stageButton = Instantiate(stageButtonPrefab, transform);
            StageButtonPanel buttonPanel = stageButton.GetComponent<StageButtonPanel>();

            // 현재 생성하는 버튼의 스테이지 ID 가져오기
            int currentStageId = stageList.items[i].stageId;
            string currentStageStatus = stageList.items[i].status;

            // 배치 예시 (지그재그 위치)
            float dValue = 100f; // 위치 간격
            float dx = 0f;       // 기본값 (중앙 배치 시 이동 x)

            int d = i % 4;
            if (d == 0) // 왼쪽
            {
                dx = -dValue;
            }
            else if (d == 2) // 오른쪽
            {
                dx = dValue;
            }

            buttonPanel.SetStageButton(dx, currentStageId, currentStageStatus);
        }
    }
}

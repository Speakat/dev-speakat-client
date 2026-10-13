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
            StageStatus currentStageStatus = stageList.items[i].status;
            string currentStageTitle = stageList.items[i].title;

            // 배치 예시 (지그재그 위치)
            float dValue = 180f; // 위치 간격
            float dx = 0f;       // 기본값

            int d = i % 2;
            if (d == 0) // 왼쪽
            {
                dx = -dValue;
            }
            else if (d == 1) // 오른쪽
            {
                dx = dValue;
            }

            if (i == 0) // 첫 번째 버튼은 선 숨기기
            {
                buttonPanel.lineImageTransform.gameObject.SetActive(false);
            }

            buttonPanel.SetStageButton(dx, currentStageId, currentStageStatus, currentStageTitle);
        }
    }
}

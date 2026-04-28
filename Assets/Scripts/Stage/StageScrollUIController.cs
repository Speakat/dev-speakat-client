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

            // 배치 예시(3가지 위치 반복)
            float dValue = 100f; // 위치 간격
            int d = i % 4;
            if (d == 0) // 왼쪽
            {
                buttonPanel.SetButtonPosition(-dValue);
            }
            else if (d == 2) // 오른쪽
            {
                buttonPanel.SetButtonPosition(dValue);
            }
            // d == 1인 경우 중앙에 배치(위치 이동 x)
        }
    }
}

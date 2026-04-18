using UnityEngine;

public class StageScrollUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject stageButtonPrefab;

    private int stageCount = 0; // 스테이지 총 개수

    private void Awake()
    {
        // TODO: 스테이지 api 호출
        stageCount = 10; // 예시로 스테이지 10개 설정

        SetPrefabs();
    }

    private void SetPrefabs()
    {
        for (int i = 0; i < stageCount; i++)
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

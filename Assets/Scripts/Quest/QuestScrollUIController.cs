using UnityEngine;

public class QuestScrollUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject questButtonPrefab;

    private int questCount = 0; // 퀘스트 총 개수

    private void Awake()
    {
        // TODO: 퀘스트 api 호출
        questCount = 10; // 예시로 퀘스트 10개 설정

        SetPrefabs();
    }

    private void SetPrefabs()
    {
        for (int i = 0; i < questCount; i++)
        {
            GameObject questButton = Instantiate(questButtonPrefab, transform);

            // 배치 예시(3가지 위치 반복)
            float dValue = 100f; // 위치 간격
            int d = i % 4;
            if (d == 0)
            {
                questButton.GetComponent<QuestButtonPanel>().SetButtonPosition(-dValue);
            }
            else if (d == 2)
            {
                questButton.GetComponent<QuestButtonPanel>().SetButtonPosition(dValue);
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

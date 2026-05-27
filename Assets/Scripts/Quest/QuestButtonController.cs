using UnityEngine;
using UnityEngine.UI;

public class QuestButtonController : MonoBehaviour
{
    private Button buttonComponent;
    private Image buttonImage;

    [SerializeField]
    private Sprite completedSprite; // 퀘스트 완료를 나타내는 이미지
    [SerializeField]
    private Sprite progressSprite; // 퀘스트 진행 중을 나타내는 이미지
    [SerializeField]
    private Sprite uncompletedSprite; // 퀘스트 미완료를 나타내는 이미지

    public int QuestId { get; private set; }
    public string QuestCompleted { get; private set; }

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    public void SetQuestButton(int id, string status)
    {
        QuestId = id;
        QuestCompleted = status;

        if (status == "COMPLETED")
        {
            buttonImage.sprite = completedSprite;
        }
        else if (status == "IN_PROGRESS")
        {
            buttonImage.sprite = progressSprite;
        }
        else if (status == "LOCKED")
        {
            buttonImage.sprite = uncompletedSprite; // 잠긴 퀘스트는 미완료 이미지로 표시
            buttonComponent.interactable = false; // 잠긴 퀘스트는 클릭 불가능
        }
        else
        {
            buttonImage.sprite = uncompletedSprite;
        }

        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(SelectQuest);
    }

    public void SelectQuest()
    {
        Debug.Log($"Selected Quest: {QuestId}, Status: {QuestCompleted}");
    }
}

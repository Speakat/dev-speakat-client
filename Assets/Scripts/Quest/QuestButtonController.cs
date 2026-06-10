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
    public bool QuestCompleted { get; private set; }

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    public void SetQuestButton(int id, bool isCompleted)
    {
        QuestId = id;
        QuestCompleted = isCompleted;

        if (isCompleted)
        {
            buttonImage.sprite = completedSprite;
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
        QuestManager.Instance.ShowQuestPopup(QuestId);
    }
}

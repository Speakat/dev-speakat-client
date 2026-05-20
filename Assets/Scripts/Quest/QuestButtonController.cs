using UnityEngine;
using UnityEngine.UI;

public class QuestButtonController : MonoBehaviour
{
    [SerializeField]
    private Button buttonComponent;

    public int QuestId { get; private set; }
    public bool QuestCompleted { get; private set; }

    public void SetQuestButton(int id, bool status)
    {
        QuestId = id;
        QuestCompleted = status;

        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(SelectQuest);
    }

    public void SelectQuest()
    {
        Debug.Log($"Selected Quest: {QuestId}, Status: {QuestCompleted}");
    }
}

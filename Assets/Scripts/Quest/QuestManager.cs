using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set;}

    [SerializeField]
    private GameObject questPopup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ShowQuestPopup(int questId)
    {
        questPopup.SetActive(true);
        questPopup.GetComponent<QuestPopupUIController>().SetQuestPopup(questId);
    }
}

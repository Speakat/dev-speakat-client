using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TabBarController : MonoBehaviour
{
    public static TabBarController Instance { get; private set; }
    private Image questImage;
    private Image vocaImage;
    private Image myPageImage;
    public Toggle questToggle;
    public Toggle vocaToggle;
    public Toggle myPageToggle;
    public GameObject tabBarUI;

    private readonly Color32 selectedColor = new Color32(0xF4, 0x8F, 0x17, 0xFF);
    private readonly Color32 normalColor = new Color32(0xAA, 0xAA, 0xAA, 0xFF);

    private readonly string[] visibleScenes = { "StageScene", "VocabularyScene", "MyPage" };

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        questToggle.onValueChanged.RemoveAllListeners();
        vocaToggle.onValueChanged.RemoveAllListeners();
        myPageToggle.onValueChanged.RemoveAllListeners();

        UpdateToggleByScene(scene.name);
        UpdateTabBarVisibility(scene.name);

        questToggle.onValueChanged.AddListener(OnQuestToggleValueChanged);
        vocaToggle.onValueChanged.AddListener(OnVocaToggleValueChanged);
        myPageToggle.onValueChanged.AddListener(OnMyPageToggleValueChanged);
    }

    private void UpdateTabBarVisibility(string sceneName)
    {
        bool shouldShow = System.Array.IndexOf(visibleScenes, sceneName) != -1;
        tabBarUI.SetActive(shouldShow);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        tabBarUI = gameObject;

        questImage = questToggle.gameObject.GetComponentInChildren<Image>();
        vocaImage = vocaToggle.gameObject.GetComponentInChildren<Image>();
        myPageImage = myPageToggle.gameObject.GetComponentInChildren<Image>();

        questToggle.SetIsOnWithoutNotify(false);
        vocaToggle.SetIsOnWithoutNotify(false);
        myPageToggle.SetIsOnWithoutNotify(false);

        questToggle.onValueChanged.AddListener(OnQuestToggleValueChanged);
        vocaToggle.onValueChanged.AddListener(OnVocaToggleValueChanged);
        myPageToggle.onValueChanged.AddListener(OnMyPageToggleValueChanged);

        UpdateToggleByScene(SceneManager.GetActiveScene().name);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void UpdateToggleByScene(string sceneName)
    {
        Debug.Log("toggle: " + sceneName);
        bool isQuest = sceneName == "StageScene";
        bool isVoca = sceneName == "VocabularyScene";
        bool isMyPage = sceneName == "MyPage";

        questToggle.SetIsOnWithoutNotify(isQuest);
        vocaToggle.SetIsOnWithoutNotify(isVoca);
        myPageToggle.SetIsOnWithoutNotify(isMyPage);

        questImage.color = isQuest ? selectedColor : normalColor;
        vocaImage.color = isVoca ? selectedColor : normalColor;
        myPageImage.color = isMyPage ? selectedColor : normalColor;
    }

    private void OnQuestToggleValueChanged(bool isOn)
    {
        if (isOn) SceneManager.LoadScene("StageScene");
    }

    private void OnVocaToggleValueChanged(bool isOn)
    {
        if (isOn) SceneManager.LoadScene("VocabularyScene");
    }

    private void OnMyPageToggleValueChanged(bool isOn)
    {
        if (isOn) SceneManager.LoadScene("MyPage");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (questToggle) questToggle.onValueChanged.RemoveListener(OnQuestToggleValueChanged);
        if (vocaToggle) vocaToggle.onValueChanged.RemoveListener(OnVocaToggleValueChanged);
        if (myPageToggle) myPageToggle.onValueChanged.RemoveListener(OnMyPageToggleValueChanged);
    }
}

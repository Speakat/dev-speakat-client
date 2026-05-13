using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex > 0) SceneManager.LoadScene(currentSceneIndex - 1);
        else Debug.LogWarning("First Scene");
    }

    // 특정 씬 로드되게 하드코딩
    public void LoadLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
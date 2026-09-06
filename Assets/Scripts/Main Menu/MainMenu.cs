using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelPanel;

    // Opens Level Selection Panel
    public void OpenLevelPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelPanel != null) levelPanel.SetActive(true);
    }

    // Returns to Main Menu Panel
    public void BackToMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelPanel != null) levelPanel.SetActive(false);
    }

    // Generic Level Loader - pass scene name directly in Unity Inspector
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}


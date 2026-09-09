using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPauseController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Pause UI")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Audio Toggles")]
    public Toggle musicToggle;
    public Toggle sfxToggle;

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        IsPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        RefreshAudioToggles();
    }

    // =========================
    // PAUSE
    // =========================

    public void PauseGame()
    {
        isPaused = true;
        IsPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Debug.Log("GAME PAUSED");
    }

    // =========================
    // RESUME
    // =========================

    public void ResumeGame()
    {
        isPaused = false;
        IsPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Debug.Log("GAME RESUMED");
    }

    // =========================
    // OPTIONS
    // =========================

    public void OpenOptions()
    {
        if (!isPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        RefreshAudioToggles();

        Debug.Log("OPTIONS OPENED");
    }

    // =========================
    // BACK TO PAUSE
    // =========================

    public void BackToPause()
    {
        if (!isPaused)
            return;

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Debug.Log("BACK TO PAUSE MENU");
    }

    // =========================
    // MUSIC
    // =========================

    public void ToggleMusic(bool isOn)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ToggleMusic(isOn);

            Debug.Log(
                "MUSIC: " +
                (isOn ? "ON" : "OFF")
            );
        }
    }

    // =========================
    // SFX
    // =========================

    public void ToggleSFX(bool isOn)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ToggleSFX(isOn);

            Debug.Log(
                "SFX: " +
                (isOn ? "ON" : "OFF")
            );
        }
    }

    // =========================
    // RESTART LEVEL
    // =========================

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        Debug.Log("RESTARTING LEVEL...");

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // =========================
    // EXIT TO MAIN MENU
    // =========================

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        Debug.Log("EXITING TO MAIN MENU...");

        SceneManager.LoadScene("Main Menu");
    }

    // =========================
    // AUDIO TOGGLE REFRESH
    // =========================

    private void RefreshAudioToggles()
    {
        // Intentionally left empty.
        // AudioManager does not expose its current
        // mute states, so we do not modify it.
    }

    // =========================
    // SAFETY RESET
    // =========================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}
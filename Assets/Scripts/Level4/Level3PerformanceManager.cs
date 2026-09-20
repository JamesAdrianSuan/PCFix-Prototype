using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Level3PerformanceManager : MonoBehaviour
{
    [Header("Managers")]
    public DisassemblyManager disassemblyManager;
    public AssemblyManager assemblyManager;
    public Level3AssemblyInitializer assemblyInitializer;
    public TimeTrialTimer timeTrialTimer;

    [Header("Instructions")]
    public GameObject instructionsPanel;
    public Button startButton;

    [Header("Level 3 Disassembly Guide UI")]
    public GameObject disassemblyInstruction;
    public GameObject progressText;

    [Header("Assembly Target Colliders")]
    public Level3AssemblyTargetColliderController assemblyTargetColliderController;

    [Header("Assembly Visuals")]
    public Level3AssemblyVisualController assemblyVisualController;

    [Header("Final Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text resultsTitle;
    public TMP_Text starText;
    public TMP_Text performanceText;
    public TMP_Text finalScoreText;
    public TMP_Text finalTimeText;
    public TMP_Text finalXPText;
    public TMP_Text finalCorrectText;
    public TMP_Text finalMistakesText;
    public TMP_Text finalAccuracyText;

    [Header("Result Buttons")]
    public Button retryButton;
    public Button mainMenuButton;

    private bool challengeStarted = false;
    private bool assemblyStarted = false;
    private bool challengeFinished = false;

    private int disassemblyCorrect = 0;
    private int disassemblyMistakes = 0;

    private int assemblyCorrect = 0;
    private int assemblyMistakes = 0;


    private void Awake()
    {
        /*
         * Level 3 starts with Disassembly only.
         */

        if (assemblyManager != null)
            assemblyManager.enabled = false;

        if (assemblyInitializer != null)
            assemblyInitializer.enabled = false;

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        /*
         * Assembly target colliders must NOT interfere
         * with the Disassembly phase.
         */
        if (assemblyTargetColliderController != null)
            assemblyTargetColliderController.DisableAssemblyTargets();
    }


    private void Start()
    {
        /*
         * Reset timer.
         */
        if (timeTrialTimer != null)
            timeTrialTimer.ResetTimer();

        /*
         * Start button.
         */
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(
                StartPerformanceChallenge
            );
        }

        /*
         * Retry button.
         */
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(
                RetryLevel
            );
        }

        /*
         * Main Menu button.
         */
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(
                BackToMainMenu
            );
        }
    }


    private void Update()
    {
        if (!challengeStarted)
            return;

        if (challengeFinished)
            return;

        /*
         * Wait for Disassembly to finish.
         */
        if (!assemblyStarted &&
            disassemblyManager != null &&
            disassemblyManager.IsDisassemblyComplete())
        {
            CaptureDisassemblyResults();
            StartAssemblyPhase();
        }

        /*
         * Wait for Assembly to finish.
         */
        if (assemblyStarted &&
            assemblyManager != null &&
            assemblyManager.IsAssemblyComplete())
        {
            CaptureAssemblyResults();
            FinishPerformanceChallenge();
        }
    }


    public void StartPerformanceChallenge()
    {
        if (challengeStarted)
            return;

        challengeStarted = true;

        Debug.Log(
            "=== LEVEL 3 PERFORMANCE CHALLENGE STARTED ==="
        );

        /*
         * Hide instruction panel.
         */
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        /*
         * Hide Disassembly step-by-step guide.
         * Level 3 is a performance challenge,
         * so the player should not receive the
         * normal procedural guide.
         */
        if (disassemblyInstruction != null)
            disassemblyInstruction.SetActive(false);

        if (progressText != null)
            progressText.SetActive(false);

        /*
         * Start count-up timer.
         */
        if (timeTrialTimer != null)
            timeTrialTimer.StartTimer();

        /*
         * Start Disassembly.
         */
        if (disassemblyManager != null)
            disassemblyManager.StartGameplay();
    }


    private void CaptureDisassemblyResults()
    {
        if (disassemblyManager == null)
            return;

        disassemblyCorrect =
            GetNumberFromText(
                disassemblyManager.finalCorrectText
            );

        disassemblyMistakes =
            GetNumberFromText(
                disassemblyManager.finalMistakesText
            );

        Debug.Log(
            "LEVEL 3 DISASSEMBLY COMPLETE | " +
            "Correct: " + disassemblyCorrect +
            " | Mistakes: " + disassemblyMistakes
        );
    }


    private void StartAssemblyPhase()
    {
        if (assemblyStarted)
            return;

        assemblyStarted = true;

        Debug.Log(
            "=== STARTING ASSEMBLY PHASE ==="
        );

        /*
         * Enable AssemblyManager.
         *
         * Its normal Start() will initialize
         * the Assembly phase.
         */
        if (assemblyManager != null)
            assemblyManager.enabled = true;

        /*
         * Enable Assembly Initializer.
         */
        if (assemblyInitializer != null)
            assemblyInitializer.enabled = true;

        /*
         * Enable Assembly target colliders.
         *
         * These were disabled during Disassembly
         * so they cannot intercept clicks.
         */
        if (assemblyTargetColliderController != null)
            assemblyTargetColliderController.EnableAssemblyTargets();

        /*
         * Restore Assembly visual objects.
         */
        if (assemblyVisualController != null)
            assemblyVisualController.EnableAllAssemblyVisuals();
    }


    private void CaptureAssemblyResults()
    {
        if (assemblyManager == null)
            return;

        assemblyCorrect =
            GetNumberFromText(
                assemblyManager.finalCorrectText
            );

        assemblyMistakes =
            GetNumberFromText(
                assemblyManager.finalMistakesText
            );

        Debug.Log(
            "LEVEL 3 ASSEMBLY COMPLETE | " +
            "Correct: " + assemblyCorrect +
            " | Mistakes: " + assemblyMistakes
        );
    }


    private void FinishPerformanceChallenge()
    {
        if (challengeFinished)
            return;

        challengeFinished = true;

        /*
         * Stop timer.
         */
        if (timeTrialTimer != null)
            timeTrialTimer.StopTimer();

        /*
         * Combined performance.
         */
        int totalCorrect =
            disassemblyCorrect +
            assemblyCorrect;

        int totalMistakes =
            disassemblyMistakes +
            assemblyMistakes;

        int totalActions =
            totalCorrect +
            totalMistakes;

        float accuracy = 0f;

        if (totalActions > 0)
        {
            accuracy =
                ((float)totalCorrect /
                totalActions) * 100f;
        }

        /*
         * Score.
         *
         * Accuracy is the main performance factor.
         * Completion time is recorded separately.
         */
        int finalScore =
            Mathf.RoundToInt(accuracy);

        /*
         * XP based on final score.
         */
        int finalXP = finalScore;

        /*
         * Star rating.
         */
        string stars;

        if (accuracy >= 90f)
        {
            stars = "★★★";
        }
        else if (accuracy >= 75f)
        {
            stars = "★★";
        }
        else if (accuracy >= 60f)
        {
            stars = "★";
        }
        else
        {
            stars = "☆";
        }

        /*
         * Display final results.
         */
        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultsTitle != null)
            resultsTitle.text =
                "PERFORMANCE CHALLENGE COMPLETE!";

        if (starText != null)
            starText.text = stars;

        if (performanceText != null)
            performanceText.text =
                "Combined Performance";

        if (finalScoreText != null)
            finalScoreText.text =
                finalScore.ToString();

        if (finalTimeText != null &&
            timeTrialTimer != null)
        {
            finalTimeText.text =
                timeTrialTimer.GetFormattedTime();
        }

        if (finalXPText != null)
            finalXPText.text =
                finalXP.ToString();

        if (finalCorrectText != null)
            finalCorrectText.text =
                totalCorrect.ToString();

        if (finalMistakesText != null)
            finalMistakesText.text =
                totalMistakes.ToString();

        if (finalAccuracyText != null)
            finalAccuracyText.text =
                accuracy.ToString("F1") + "%";

        Debug.Log(
            "=== LEVEL 3 PERFORMANCE COMPLETE ==="
        );

        Debug.Log(
            "Total Correct: " +
            totalCorrect
        );

        Debug.Log(
            "Total Mistakes: " +
            totalMistakes
        );

        Debug.Log(
            "Accuracy: " +
            accuracy.ToString("F1") +
            "%"
        );

        Debug.Log(
            "Final Time: " +
            (timeTrialTimer != null
                ? timeTrialTimer.GetFormattedTime()
                : "N/A")
        );

        Debug.Log(
            "Final Score: " +
            finalScore
        );

        Debug.Log(
            "Final XP: " +
            finalXP
        );
    }


    private int GetNumberFromText(TMP_Text text)
    {
        if (text == null)
            return 0;

        string value = text.text;

        string number = "";

        foreach (char character in value)
        {
            if (char.IsDigit(character))
            {
                number += character;
            }
            else if (number.Length > 0)
            {
                break;
            }
        }

        if (int.TryParse(number, out int result))
            return result;

        return 0;
    }


    public void RetryLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }


    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "Main Menu"
        );
    }
}
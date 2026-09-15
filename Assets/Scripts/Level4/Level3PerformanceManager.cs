using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Level3PerformanceManager : MonoBehaviour
{
    [Header("Managers")]
    public DisassemblyManager disassemblyManager;
    public AssemblyManager assemblyManager;
    public Level3AssemblyInitializer assemblyInitializer;

    [Header("Timer")]
    public TimeTrialTimer timeTrialTimer;

    [Header("Level 3 UI")]
    public GameObject instructionsPanel;
    public Button startButton;

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

    [Header("Assembly Target Colliders")]
    public Level3AssemblyTargetColliderController assemblyTargetColliderController;

    [Header("Assembly Visuals")]
    public Level3AssemblyVisualController assemblyVisualController;

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
         * Prevent AssemblyManager from starting
         * before the Disassembly phase.
         */
        if (assemblyManager != null)
            assemblyManager.enabled = false;

        /*
         * Prevent the Assembly Initializer from
         * running before the Assembly phase.
         */
        if (assemblyInitializer != null)
            assemblyInitializer.enabled = false;

        /*
         * Hide the final Level 3 Results Panel.
         */
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    private void Start()
    {
        /*
         * Make sure timer starts at 00:00.
         */
        if (timeTrialTimer != null)
            timeTrialTimer.ResetTimer();

        /*
         * Connect Start button.
         */
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(
                StartPerformanceChallenge
            );
        }

        /*
         * Connect Retry button.
         */
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(
                RetryLevel
            );
        }

        /*
         * Connect Main Menu button.
         */
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(
                BackToMainMenu
            );
        }

        Debug.Log(
            "=== LEVEL 3 PERFORMANCE MANAGER READY ==="
        );
    }

    private void Update()
    {
        if (!challengeStarted ||
            challengeFinished)
            return;

        /*
         * DISASSEMBLY COMPLETE
         */
        if (!assemblyStarted &&
            disassemblyManager != null &&
            disassemblyManager.IsDisassemblyComplete())
        {
            HandleDisassemblyComplete();
        }

        /*
         * ASSEMBLY COMPLETE
         */
        if (assemblyStarted &&
            assemblyManager != null &&
            assemblyManager.IsAssemblyComplete())
        {
            HandleAssemblyComplete();
        }
    }

    public void StartPerformanceChallenge()
    {
        if (challengeStarted)
            return;

        challengeStarted = true;
        assemblyStarted = false;
        challengeFinished = false;

        Debug.Log(
            "=== PERFORMANCE CHALLENGE STARTED ==="
        );

        /*
         * Hide Level 3 introduction/instructions.
         */
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        /*
         * Hide final results.
         */
        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        /*
         * Start ONE continuous timer.
         */
        if (timeTrialTimer != null)
        {
            timeTrialTimer.ResetTimer();
            timeTrialTimer.StartTimer();
        }

        /*
         * Start DISASSEMBLY first.
         */
        if (disassemblyManager != null)
        {
            disassemblyManager.StartGameplay();

            Debug.Log(
                "=== DISASSEMBLY PHASE STARTED ==="
            );
        }
    }

    private void HandleDisassemblyComplete()
    {
        Debug.Log(
            "=== DISASSEMBLY PHASE COMPLETE ==="
        );

        /*
         * Capture Disassembly results.
         */
        disassemblyCorrect =
            GetNumberFromText(
                disassemblyManager.finalCorrectText
            );

        disassemblyMistakes =
            GetNumberFromText(
                disassemblyManager.finalMistakesText
            );

        Debug.Log(
            "Disassembly Correct: " +
            disassemblyCorrect
        );

        Debug.Log(
            "Disassembly Mistakes: " +
            disassemblyMistakes
        );

        /*
         * Start Assembly.
         */
        StartAssemblyPhase();
    }

    private void StartAssemblyPhase()
    {
        if (assemblyStarted)
            return;

        assemblyStarted = true;

        Debug.Log(
            "=== STARTING ASSEMBLY PHASE ==="
        );

        if (assemblyManager != null)
            assemblyManager.enabled = true;

        if (assemblyInitializer != null)
            assemblyInitializer.enabled = true;

        if (assemblyTargetColliderController != null)
            assemblyTargetColliderController.EnableAssemblyTargets();

        if (assemblyVisualController != null)
            assemblyVisualController.EnableAllAssemblyVisuals();
    }

    private void HandleAssemblyComplete()
    {
        Debug.Log(
            "=== ASSEMBLY PHASE COMPLETE ==="
        );

        /*
         * Capture Assembly results.
         */
        assemblyCorrect =
            GetNumberFromText(
                assemblyManager.finalCorrectText
            );

        assemblyMistakes =
            GetNumberFromText(
                assemblyManager.finalMistakesText
            );

        Debug.Log(
            "Assembly Correct: " +
            assemblyCorrect
        );

        Debug.Log(
            "Assembly Mistakes: " +
            assemblyMistakes
        );

        FinishPerformanceChallenge();
    }

    private void FinishPerformanceChallenge()
    {
        if (challengeFinished)
            return;

        challengeFinished = true;

        /*
         * STOP ONE CONTINUOUS TIMER.
         */
        if (timeTrialTimer != null)
            timeTrialTimer.StopTimer();

        /*
         * COMBINE BOTH PHASES.
         */
        int totalCorrect =
            disassemblyCorrect +
            assemblyCorrect;

        int totalMistakes =
            disassemblyMistakes +
            assemblyMistakes;

        int totalInteractions =
            totalCorrect +
            totalMistakes;

        float accuracy =
            totalInteractions > 0
                ? ((float)totalCorrect /
                   totalInteractions) * 100f
                : 0f;

        int score =
            Mathf.RoundToInt(accuracy);

        int xp =
            Mathf.RoundToInt(score * 1.5f);

        int stars;

        if (accuracy >= 90f)
            stars = 3;
        else if (accuracy >= 70f)
            stars = 2;
        else
            stars = 1;

        /*
         * SHOW FINAL LEVEL 3 RESULTS.
         */
        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultsTitle != null)
        {
            resultsTitle.text =
                "PERFORMANCE CHALLENGE COMPLETE";
        }

        if (starText != null)
        {
            starText.text =
                "Stars: " +
                stars +
                "/3";
        }

        if (performanceText != null)
        {
            if (accuracy >= 90f)
            {
                performanceText.text =
                    "Excellent! Great job!";
            }
            else if (accuracy >= 70f)
            {
                performanceText.text =
                    "Good job! Keep practicing!";
            }
            else
            {
                performanceText.text =
                    "Keep practicing!";
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "Final Score: " +
                score;
        }

        if (finalTimeText != null)
        {
            finalTimeText.text =
                "Completion Time: " +
                timeTrialTimer.GetFormattedTime();
        }

        if (finalXPText != null)
        {
            finalXPText.text =
                "XP: " +
                xp;
        }

        if (finalCorrectText != null)
        {
            finalCorrectText.text =
                "Correct Interactions: " +
                totalCorrect;
        }

        if (finalMistakesText != null)
        {
            finalMistakesText.text =
                "Wrong Clicks: " +
                totalMistakes;
        }

        if (finalAccuracyText != null)
        {
            finalAccuracyText.text =
                "Accuracy: " +
                accuracy.ToString("F1") +
                "%";
        }

        Debug.Log(
            "=== FINAL PERFORMANCE RESULTS ==="
        );

        Debug.Log(
            "Correct: " +
            totalCorrect
        );

        Debug.Log(
            "Mistakes: " +
            totalMistakes
        );

        Debug.Log(
            "Accuracy: " +
            accuracy.ToString("F1") +
            "%"
        );

        Debug.Log(
            "Score: " +
            score
        );

        Debug.Log(
            "XP: " +
            xp
        );

        Debug.Log(
            "Stars: " +
            stars +
            "/3"
        );

        Debug.Log(
            "Time: " +
            timeTrialTimer.GetFormattedTime()
        );
    }

    private int GetNumberFromText(TMP_Text text)
    {
        if (text == null)
            return 0;

        string value = text.text;
        string number = "";

        foreach (char c in value)
        {
            if (char.IsDigit(c))
            {
                number += c;
            }
            else if (number.Length > 0)
            {
                break;
            }
        }

        int result;

        if (int.TryParse(
            number,
            out result))
        {
            return result;
        }

        return 0;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AssemblyManager : MonoBehaviour
{
    [System.Serializable]
    public class AssemblyGroup
    {
        public string componentName;
        public AssemblyComponent component;
    }

    [Header("UI")]
    public TMP_Text instructionText;
    public TMP_Text progressText;

    [Header("Assembly Components")]
    public AssemblyGroup[] components;

    [Header("Wrong Click Feedback")]
    public GameObject wrongClickFeedback;
    public TMP_Text wrongClickFeedbackText;
    public float feedbackDuration = 1.5f;

    [Header("Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text resultsTitle;
    public TMP_Text starText;
    public TMP_Text performanceText;
    public TMP_Text finalScoreText;
    public TMP_Text finalMistakesText;
    public TMP_Text finalAccuracyText;
    public Button retryButton;

    [Header("Optional Results")]
    public TMP_Text finalCorrectText;
    public TMP_Text finalXPText;

    private int currentStep = 0;

    private int correctCount = 0;
    private int mistakeCount = 0;

    private Coroutine feedbackCoroutine;

    private void Start()
    {
        Debug.Log("=== ASSEMBLY MANAGER STARTED ===");

        correctCount = 0;
        mistakeCount = 0;
        currentStep = 0;

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        if (wrongClickFeedback != null)
            wrongClickFeedback.SetActive(false);

        DisableAllComponents();
        SetupCurrentStep();
    }

    private void DisableAllComponents()
    {
        if (components == null)
        {
            Debug.LogError(
                "Assembly Components array is NULL!"
            );

            return;
        }

        foreach (AssemblyGroup group in components)
        {
            if (group == null ||
                group.component == null)
                continue;

            group.component.Setup(this);
            group.component.SetInteractable(false);
        }
    }

    private void SetupCurrentStep()
    {
        if (components == null ||
            components.Length == 0)
        {
            Debug.LogError(
                "No Assembly Components assigned!"
            );

            return;
        }

        if (currentStep >= components.Length)
        {
            CompleteAssembly();
            return;
        }

        AssemblyGroup currentGroup =
            components[currentStep];

        if (currentGroup == null)
            return;

        Debug.Log(
            "CURRENT ASSEMBLY STEP: " +
            (currentStep + 1) +
            " / " +
            components.Length +
            " | TARGET: " +
            currentGroup.componentName
        );

        DisableAllComponents();

        if (currentGroup.component != null)
        {
            currentGroup.component.Setup(this);
            currentGroup.component.SetInteractable(true);
        }

        UpdateInstruction(
            "STEP " +
            (currentStep + 1) +
            "\n\nInstall the " +
            currentGroup.componentName
        );
    }

    public bool IsCorrectAssemblyStep(
        string componentName)
    {
        if (components == null ||
            components.Length == 0)
            return false;

        if (currentStep >= components.Length)
            return false;

        if (string.IsNullOrWhiteSpace(componentName))
            return false;

        AssemblyGroup currentGroup =
            components[currentStep];

        if (currentGroup == null)
            return false;

        return string.Equals(
            componentName.Trim(),
            currentGroup.componentName.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );
    }

    public string GetCurrentExpectedComponent()
    {
        if (components == null ||
            components.Length == 0)
            return "";

        if (currentStep >= components.Length)
            return "";

        AssemblyGroup currentGroup =
            components[currentStep];

        if (currentGroup == null)
            return "";

        return currentGroup.componentName;
    }

    public int GetCurrentStep()
    {
        return currentStep;
    }

    // =========================================================
    // MISTAKE / WRONG DROP
    // =========================================================

    public void RecordMistake(string message)
    {
        mistakeCount++;

        Debug.Log(
            "ASSEMBLY MISTAKE #" +
            mistakeCount +
            " | " +
            message
        );

        ShowWrongFeedback(message);
    }

    private void ShowWrongFeedback(string message)
    {
        if (wrongClickFeedback == null)
            return;

        if (wrongClickFeedbackText != null)
            wrongClickFeedbackText.text = message;

        wrongClickFeedback.SetActive(true);

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine =
            StartCoroutine(
                HideWrongFeedback()
            );
    }

    private System.Collections.IEnumerator HideWrongFeedback()
    {
        yield return new WaitForSeconds(
            feedbackDuration
        );

        if (wrongClickFeedback != null)
            wrongClickFeedback.SetActive(false);

        feedbackCoroutine = null;
    }

    // =========================================================
    // DIRECT COMPONENT CLICK
    // =========================================================

    public void ComponentClicked(
        AssemblyComponent component)
    {
        if (component == null)
            return;

        if (components == null ||
            currentStep >= components.Length)
            return;

        AssemblyGroup currentGroup =
            components[currentStep];

        if (currentGroup == null)
            return;

        Debug.Log(
            "CHECKING ASSEMBLY COMPONENT | " +
            "Clicked: [" +
            component.componentName +
            "] | Expected: [" +
            currentGroup.componentName +
            "]"
        );

        if (string.Equals(
            component.componentName.Trim(),
            currentGroup.componentName.Trim(),
            System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log(
                "CORRECT ASSEMBLY COMPONENT: " +
                currentGroup.componentName
            );

            CompleteCurrentStep(component);
        }
        else
        {
            RecordMistake(
                "Wrong component! Install the " +
                currentGroup.componentName
            );
        }
    }

    private void CompleteCurrentStep(
        AssemblyComponent component)
    {
        if (component == null)
            return;

        AssemblyGroup completedGroup =
            components[currentStep];

        Debug.Log(
            "ASSEMBLY STEP " +
            (currentStep + 1) +
            " COMPLETE: " +
            completedGroup.componentName
        );

        correctCount++;

        component.SetInteractable(false);
        component.Assemble();

        currentStep++;

        if (currentStep >= components.Length)
        {
            CompleteAssembly();
            return;
        }

        SetupCurrentStep();
    }

    // =========================================================
    // INVENTORY DRAG / DROP
    // =========================================================

    public bool CompleteFromInventory(
        string componentName,
        AssemblyDropTarget target)
    {
        if (target == null)
        {
            RecordMistake(
                "Invalid assembly target."
            );

            return false;
        }

        if (!IsCorrectAssemblyStep(componentName))
        {
            RecordMistake(
                "Wrong component! Install the " +
                GetCurrentExpectedComponent()
            );

            Debug.Log(
                "WRONG ASSEMBLY STEP | " +
                "Expected: " +
                GetCurrentExpectedComponent() +
                " | Dragged: " +
                componentName
            );

            return false;
        }

        if (!target.IsCorrectComponent(componentName))
        {
            RecordMistake(
                "Wrong location! Place the " +
                componentName +
                " in the correct area."
            );

            Debug.Log(
                "WRONG ASSEMBLY TARGET | " +
                "Expected target for: " +
                componentName
            );

            return false;
        }

        Debug.Log(
            "CORRECT INVENTORY ASSEMBLY: " +
            componentName
        );

        target.PlaceComponent();

        correctCount++;

        currentStep++;

        if (currentStep >= components.Length)
        {
            CompleteAssembly();
        }
        else
        {
            SetupCurrentStep();
        }

        return true;
    }

    // =========================================================
    // UI
    // =========================================================

    private void UpdateInstruction(
        string message)
    {
        if (instructionText != null)
            instructionText.text = message;

        if (progressText != null)
        {
            if (currentStep >= components.Length)
            {
                progressText.text = "Complete!";
            }
            else
            {
                progressText.text =
                    "Step " +
                    (currentStep + 1) +
                    " / " +
                    components.Length;
            }
        }
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteAssembly()
    {
        if (instructionText != null)
            instructionText.text =
                "ASSEMBLY COMPLETE!";

        if (progressText != null)
            progressText.text =
                "Complete!";

        Debug.Log(
            "=== ALL ASSEMBLY STEPS COMPLETE ==="
        );

        ShowResultsPanel();
    }

    // =========================================================
    // RESULTS
    // =========================================================

    private void ShowResultsPanel()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        int totalInteractions =
            correctCount + mistakeCount;

        float accuracy =
            totalInteractions > 0
                ? ((float)correctCount /
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

        if (resultsTitle != null)
            resultsTitle.text =
                "ASSEMBLY COMPLETE";

        if (starText != null)
            starText.text =
                "Stars: " + stars + "/3";

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
            finalScoreText.text =
                "Score: " + score;

        if (finalMistakesText != null)
            finalMistakesText.text =
                "Wrong Clicks: " +
                mistakeCount;

        if (finalAccuracyText != null)
        {
            finalAccuracyText.text =
                "Accuracy: " +
                accuracy.ToString("F1") +
                "%";
        }

        if (finalCorrectText != null)
            finalCorrectText.text =
                "Correct Interactions: " +
                correctCount;

        if (finalXPText != null)
            finalXPText.text =
                "XP: " + xp;

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();

            retryButton.onClick.AddListener(
                RetryLevel
            );
        }

        Debug.Log(
            "=== ASSEMBLY RESULTS ==="
        );

        Debug.Log(
            "Correct: " +
            correctCount
        );

        Debug.Log(
            "Mistakes: " +
            mistakeCount
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
            "Stars: " +
            stars +
            "/3"
        );
    }

    // =========================================================
    // RETRY
    // =========================================================

    public void RetryLevel()
    {
        Debug.Log(
            "RESTARTING LEVEL 3..."
        );

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }
    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            "Main Menu"
        );
    }
}
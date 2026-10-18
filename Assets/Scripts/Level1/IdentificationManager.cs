using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IdentificationManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        [TextArea(2, 4)]
        public string questionText;
        public string[] options = new string[4];
        public int correctAnswerIndex;

        [Header("Component Info & 2D Sprite Preview")]
        public string componentName;
        [TextArea(3, 5)]
        public string componentDescription;
        public Sprite componentSprite;

        [Header("Feedback / Explanation")]
        [TextArea(2, 4)]
        public string wrongAnswerFeedback = "Wrong answer. Please review the correct component.";
    }

    [Header("UI References")]
    public TMP_Text questionTextDisplay;
    public TMP_Text questionNumberDisplay;
    public TMP_Text scoreDisplay;
    public TMP_Text timerTextDisplay; // Bagong UI text para sa 10-second timer
    public Button[] answerButtons = new Button[4];
    public TMP_Text[] buttonTexts = new TMP_Text[4];

    [Header("Button Colors")]
    public Color normalButtonColor = new Color(0.1f, 0.15f, 0.3f);
    public Color correctColor = new Color(0.1f, 0.6f, 0.2f);
    public Color wrongColor = new Color(0.8f, 0.2f, 0.2f);

    [Header("Inspection UI References")]
    public GameObject inspectionPanel;
    public TMP_Text itemTitleText;
    public TMP_Text itemDescriptionText;
    public Image centerImagePreview;
    public Button nextButton;

    [Header("Instructions Panel UI References")]
    public GameObject instructionsPanel;
    public Button startButton;

    [Header("Results Panel UI References")]
    public GameObject resultsPanel;
    public TMP_Text resultsTitleText;
    public TMP_Text starText;
    public TMP_Text performanceText;
    public TMP_Text finalScoreText;
    public TMP_Text finalXPText;
    public TMP_Text finalCorrectText;
    public TMP_Text finalMistakesText;
    public TMP_Text finalAccuracyText;
    public Button retryButton;
    public Button nextLevelButton;

    [Header("Next Level Configuration")]
    public string nextLevelSceneName = "Level2_Disassembly";

    [Header("Questions Database")]
    public List<QuestionData> questionsList = new List<QuestionData>();

    private int currentQuestionIndex = 0;
    private int score = 0;
    private int correctAnswersCount = 0;
    private int mistakesCount = 0;
    private bool isAnswerLocked = false;

    // Timer Variables
    private float timeRemaining = 10f;
    private bool timerIsRunning = false;

    void Start()
    {
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
        if (centerImagePreview != null) centerImagePreview.gameObject.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartGameClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
            nextButton.gameObject.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RestartLevel);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    void Update()
    {
        // Countdown timer logic
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();

                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    timerIsRunning = false;
                    OnTimeOut(); // Kapag naubos ang oras
                }
            }
        }
    }

    void OnStartGameClicked()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
        Time.timeScale = 1f;

        LoadQuestion(currentQuestionIndex);
        UpdateScoreUI();
    }

    void LoadQuestion(int index)
    {
        isAnswerLocked = false;

        if (inspectionPanel != null) inspectionPanel.SetActive(false);
        if (centerImagePreview != null) centerImagePreview.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);

        if (index < 0 || index >= questionsList.Count)
        {
            EndLevel();
            return;
        }

        QuestionData currentQ = questionsList[index];

        if (questionTextDisplay != null)
        {
            questionTextDisplay.text = currentQ.questionText;
            questionTextDisplay.color = Color.white;
        }

        if (questionNumberDisplay != null) questionNumberDisplay.text = $"Question {index + 1} of {questionsList.Count}";

        string[] prefixes = { "A: ", "B: ", "C: ", "D: " };
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQ.options.Length)
            {
                if (buttonTexts[i] != null) buttonTexts[i].text = prefixes[i] + currentQ.options[i];
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].interactable = true;

                Image btnImage = answerButtons[i].GetComponent<Image>();
                if (btnImage != null) btnImage.color = normalButtonColor;
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // I-reset at simulan ang 10-second timer
        timeRemaining = 10f;
        timerIsRunning = true;
    }

    void UpdateTimerUI()
    {
        if (timerTextDisplay != null)
        {
            timerTextDisplay.text = $"Time: {Mathf.Ceil(timeRemaining)}s";
        }
    }

    void OnTimeOut()
    {
        if (isAnswerLocked) return;
        isAnswerLocked = true;
        timerIsRunning = false;

        QuestionData currentQ = questionsList[currentQuestionIndex];

        foreach (var btn in answerButtons)
        {
            btn.interactable = false;
        }

        // Ituring na mali kapag naubusan ng oras
        mistakesCount++;

        if (inspectionPanel != null) inspectionPanel.SetActive(true);

        if (centerImagePreview != null)
        {
            if (currentQ.componentSprite != null)
            {
                centerImagePreview.gameObject.SetActive(true);
                centerImagePreview.sprite = currentQ.componentSprite;
                centerImagePreview.preserveAspect = true;
            }
            else
            {
                centerImagePreview.gameObject.SetActive(false);
            }
        }

        if (itemTitleText != null) itemTitleText.text = currentQ.componentName;
        if (itemDescriptionText != null) itemDescriptionText.text = currentQ.componentDescription;

        Image correctImage = answerButtons[currentQ.correctAnswerIndex].GetComponent<Image>();
        if (correctImage != null) correctImage.color = correctColor;

        if (questionTextDisplay != null)
        {
            questionTextDisplay.text = "Time's up! " + currentQ.wrongAnswerFeedback;
            questionTextDisplay.color = Color.red;
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    void OnAnswerSelected(int selectedIndex)
    {
        if (isAnswerLocked) return;
        isAnswerLocked = true;
        timerIsRunning = false; // Itigil ang timer kapag nakapili na

        QuestionData currentQ = questionsList[currentQuestionIndex];

        foreach (var btn in answerButtons)
        {
            btn.interactable = false;
        }

        Image selectedImage = answerButtons[selectedIndex].GetComponent<Image>();

        if (inspectionPanel != null) inspectionPanel.SetActive(true);

        if (centerImagePreview != null)
        {
            if (currentQ.componentSprite != null)
            {
                centerImagePreview.gameObject.SetActive(true);
                centerImagePreview.sprite = currentQ.componentSprite;
                centerImagePreview.preserveAspect = true;
            }
            else
            {
                centerImagePreview.gameObject.SetActive(false);
            }
        }

        if (itemTitleText != null) itemTitleText.text = currentQ.componentName;
        if (itemDescriptionText != null) itemDescriptionText.text = currentQ.componentDescription;

        if (selectedIndex == currentQ.correctAnswerIndex)
        {
            if (selectedImage != null) selectedImage.color = correctColor;
            score += 100;
            correctAnswersCount++;
            UpdateScoreUI();

            if (questionTextDisplay != null)
            {
                questionTextDisplay.text = "Correct!!! Excellent job!";
                questionTextDisplay.color = Color.green;
            }
        }
        else
        {
            if (selectedImage != null) selectedImage.color = wrongColor;

            Image correctImage = answerButtons[currentQ.correctAnswerIndex].GetComponent<Image>();
            if (correctImage != null) correctImage.color = correctColor;

            mistakesCount++;

            if (questionTextDisplay != null)
            {
                questionTextDisplay.text = currentQ.wrongAnswerFeedback;
                questionTextDisplay.color = Color.red;
            }
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    void OnNextButtonClicked()
    {
        ProceedToNextQuestion();
    }

    void ProceedToNextQuestion()
    {
        currentQuestionIndex++;
        LoadQuestion(currentQuestionIndex);
    }

    void UpdateScoreUI()
    {
        if(scoreDisplay != null) scoreDisplay.text = $"Score: {score}"; // Note: Pwedeng alisin ang extra dot kung mag-error, gagamitin ang standard text format
    }

    void EndLevel()
    {
        timerIsRunning = false;
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
        if (centerImagePreview != null) centerImagePreview.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (timerTextDisplay != null) timerTextDisplay.gameObject.SetActive(false);

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);

            int totalQuestions = questionsList.Count;
            float accuracyPercentage = totalQuestions > 0 ? ((float)correctAnswersCount / totalQuestions) * 100f : 0f;
            int totalXP = score / 2;

            if (finalScoreText != null) finalScoreText.text = $"Score: {score}";
            if (finalXPText != null) finalXPText.text = $"XP Earned: +{totalXP}";
            if (finalCorrectText != null) finalCorrectText.text = $"Correct: {correctAnswersCount}";
            if (finalMistakesText != null) finalMistakesText.text = $"Mistakes: {mistakesCount}";
            if (finalAccuracyText != null) finalAccuracyText.text = $"Accuracy: {accuracyPercentage:F0}%";

            if (accuracyPercentage >= 80f)
            {
                if (starText != null) starText.text = "Stars: 3/3";
                if (performanceText != null) performanceText.text = "Excellent Work!";
            }
            else if (accuracyPercentage >= 50f)
            {
                if (starText != null) starText.text = "Stars: 2/3";
                if (performanceText != null) performanceText.text = "Good Job!";
            }
            else
            {
                if (starText != null) starText.text = "Stars: 1/3";
                if (performanceText != null) performanceText.text = "Keep Practicing!";
            }
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning("Next Level Scene Name is not assigned!");
        }
    }
}
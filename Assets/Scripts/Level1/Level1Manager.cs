
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class HardwareQuestion
{
    public string correctAnswer;

    [TextArea(2, 5)]
    public string description;

    public GameObject targetObject;
}

public class Level1Manager : MonoBehaviour
{
    [Header("Questions")]
    public List<HardwareQuestion> questions =
        new List<HardwareQuestion>();

    [Header("Question UI")]
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public TMP_Text progressText;

    [Header("Game UI")]
    public TMP_Text scoreText;
    public TMP_Text xpText;
    public TMP_Text mistakesText;

    [Header("Answer Buttons")]
    public AnswerButton[] answerButtons;

    [Header("Results UI")]
    public GameObject resultsPanel;
    public TMP_Text starText;
    public TMP_Text performanceText;
    public TMP_Text finalScoreText;
    public TMP_Text finalXPText;
    public TMP_Text finalCorrectText;
    public TMP_Text finalMistakesText;
    public TMP_Text finalAccuracyText;

    private int currentQuestion = 0;
    private int score = 0;
    private int xp = 0;
    private int mistakes = 0;
    private int correctAnswers = 0;
    private int totalAttempts = 0;

    private HardwareQuestion currentHardware;

    private void Start()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        UpdateUI();
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        if (currentQuestion >= questions.Count)
        {
            FinishLevel();
            return;
        }

        currentHardware = questions[currentQuestion];

        // Highlight the current hardware.
        if (currentHardware.targetObject != null)
        {
            HardwareComponent component =
                currentHardware.targetObject
                    .GetComponent<HardwareComponent>();

            if (component != null)
            {
                component.Highlight();
            }
        }

        // Progress text.
        if (progressText != null)
        {
            progressText.text =
                "Question " +
                (currentQuestion + 1) +
                " of " +
                questions.Count;
        }

        // Question text.
        if (questionText != null)
        {
            questionText.text =
                "What component is highlighted?";
        }

        // Clear feedback.
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        GenerateAnswers();
    }

    private void GenerateAnswers()
    {
        List<string> answers =
            new List<string>();

        // Add correct answer first.
        answers.Add(
            currentHardware.correctAnswer
        );

        // Possible answers.
        List<string> possibleAnswers =
            new List<string>
            {
                "CPU",
                "RAM",
                "Motherboard",
                "GPU",
                "PSU",
                "PC Case"
            };

        // Remove correct answer so it cannot be selected twice.
        possibleAnswers.Remove(
            currentHardware.correctAnswer
        );

        // Add three random incorrect answers.
        while (answers.Count < 4)
        {
            string randomAnswer =
                possibleAnswers[
                    Random.Range(
                        0,
                        possibleAnswers.Count
                    )
                ];

            if (!answers.Contains(randomAnswer))
            {
                answers.Add(randomAnswer);
            }
        }

        // Shuffle the four answers.
        for (int i = 0;
             i < answers.Count;
             i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    answers.Count
                );

            string temp = answers[i];

            answers[i] =
                answers[randomIndex];

            answers[randomIndex] =
                temp;
        }

        // Give answers to the buttons.
        for (int i = 0;
             i < answerButtons.Length;
             i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].Setup(
                    answers[i],
                    this
                );
            }
        }
    }

    public void CheckAnswer(string selectedAnswer)
    {
        // Count every click as an attempt.
        totalAttempts++;

        // CORRECT ANSWER
        if (selectedAnswer ==
            currentHardware.correctAnswer)
        {
            score += 10;
            xp += 10;
            correctAnswers++;

            if (feedbackText != null)
            {
                feedbackText.text =
                    "✓ Correct!\n\n" +
                    "+10 Points   +10 XP";
            }

            RemoveCurrentHighlight();

            currentQuestion++;

            UpdateUI();

            // Go to next question after 1.5 seconds.
            Invoke(
                nameof(LoadQuestion),
                1.5f
            );
        }

        // WRONG ANSWER
        else
        {
            score =
                Mathf.Max(
                    0,
                    score - 5
                );

            mistakes++;

            if (feedbackText != null)
            {
                feedbackText.text =
                    "✗ Incorrect!\n\n" +
                    "Correct answer: " +
                    currentHardware.correctAnswer;
            }

            RemoveCurrentHighlight();

            currentQuestion++;

            UpdateUI();

            // Go to next question after 1.5 seconds.
            Invoke(
                nameof(LoadQuestion),
                1.5f
            );
        }
    }

    private void RemoveCurrentHighlight()
    {
        if (currentHardware != null &&
            currentHardware.targetObject != null)
        {
            HardwareComponent component =
                currentHardware.targetObject
                    .GetComponent<HardwareComponent>();

            if (component != null)
            {
                component.RemoveHighlight();
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Score: " + score;
        }

        if (xpText != null)
        {
            xpText.text =
                "XP: " + xp;
        }

        if (mistakesText != null)
        {
            mistakesText.text =
                "Mistakes: " + mistakes;
        }
    }

   
    private void FinishLevel()
    {
        RemoveCurrentHighlight();

        // Hide question area.
        if (questionText != null)
        {
            questionText.transform
                .parent.gameObject
                .SetActive(false);
        }

        // Calculate accuracy.
        float accuracy = 0f;

        if (totalAttempts > 0)
        {
            accuracy =
                ((float)correctAnswers /
                totalAttempts) *
                100f;
        }

        // Calculate stars.
        int stars;

        if (accuracy >= 90f)
        {
            stars = 3;
        }
        else if (accuracy >= 75f)
        {
            stars = 2;
        }
        else
        {
            stars = 1;
        }

        if (performanceText != null)
        {
            if (accuracy >= 90f)
            {
                performanceText.text = "EXCELLENT!";
            }
            else if (accuracy >= 75f)
            {
                performanceText.text = "GOOD WORK!";
            }
            else
            {
                performanceText.text = "KEEP PRACTICING!";
            }
        }

        // Display stars.
        if (starText != null)
        {
            string starsDisplay = "";

            for (int i = 0; i < 3; i++)
            {
                if (i < stars)
                {
                    starsDisplay += "★ ";
                }
                else
                {
                    starsDisplay += "☆ ";
                }
            }

            starText.text = starsDisplay;
        }

        // Show Results Panel.
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        // Final Score.
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "Final Score: " + score;
        }

        // Final XP.
        if (finalXPText != null)
        {
            finalXPText.text =
                "XP Earned: " + xp;
        }

        // Correct answers.
        if (finalCorrectText != null)
        {
            finalCorrectText.text =
                "Correct: " +
                correctAnswers +
                " / " +
                questions.Count;
        }

        // Mistakes.
        if (finalMistakesText != null)
        {
            finalMistakesText.text =
                "Mistakes: " +
                mistakes;
        }

        // Accuracy.
        if (finalAccuracyText != null)
        {
            finalAccuracyText.text =
                "Accuracy: " +
                accuracy.ToString("0") +
                "%";
        }

        Debug.Log(
            "LEVEL 1 COMPLETE! " +
            "Score: " + score +
            " | XP: " + xp +
            " | Correct: " +
            correctAnswers +
            "/" +
            questions.Count +
            " | Mistakes: " +
            mistakes +
            " | Attempts: " +
            totalAttempts +
            " | Accuracy: " +
            accuracy.ToString("0") +
            "% " +
            "| Stars: " +
            stars
        );
    }

    public void RestartLevel()
    {
        // Cancel any pending next-question call.
        CancelInvoke(
            nameof(LoadQuestion)
        );

        // Reset all values.
        currentQuestion = 0;
        score = 0;
        xp = 0;
        mistakes = 0;
        correctAnswers = 0;
        totalAttempts = 0;

        // Hide results.
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        // Show question area.
        if (questionText != null)
        {
            questionText.transform
                .parent.gameObject
                .SetActive(true);
        }

        // Clear feedback.
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        UpdateUI();

        // Start again from Question 1.
        LoadQuestion();
    }
}


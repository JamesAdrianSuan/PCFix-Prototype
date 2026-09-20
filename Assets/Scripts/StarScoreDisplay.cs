using UnityEngine;
using TMPro;

public class StarScoreDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text starTextComponent;

    // Unicode Escape Sequences
    private const string SOLID_STAR = "\u2605";    // ★
    private const string OUTLINE_STAR = "\u2606";  // ☆
    private const string CHECKMARK = "\u2713";     // ✓
    private const string CROSS_MARK = "\u2717";    // ✗

    /// <summary>
    /// Updates the star rating display based on score (e.g., 0 to 3 stars).
    /// </summary>
    public void UpdateStarRating(int starCount)
    {
        string resultText = "";

        // Build 3-star rating string dynamically
        for (int i = 0; i < 3; i++)
        {
            if (i < starCount)
            {
                resultText += SOLID_STAR;
            }
            else
            {
                resultText += OUTLINE_STAR;
            }
        }

        // Apply text to TextMeshPro component
        starTextComponent.text = resultText;
    }

    /// <summary>
    /// Example method to show feedback with stars, checkmarks, or cross marks.
    /// </summary>
    public void SetFeedbackResult(bool isCorrect, int starsEarned)
    {
        string statusIcon = isCorrect ? CHECKMARK : CROSS_MARK;

        string stars = "";
        for (int i = 0; i < starsEarned; i++)
        {
            stars += SOLID_STAR;
        }

        // Output example: "✓ Passed! ★★★"
        starTextComponent.text = $"{statusIcon} Passed! {stars}";
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MillionaireAnswerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] answerButtons;             // Ang 4 na Answer Buttons
    public Image darkBackgroundPanel;          // Ang dark background panel sa ibaba

    [Header("Millionaire Style Colors")]
    public Color normalButtonColor = new Color(0.05f, 0.1f, 0.3f, 1f); // Dark Blue
    public Color selectedColor = new Color(0.9f, 0.6f, 0.1f, 1f);     // Gold/Orange (Final Answer)
    public Color correctColor = new Color(0.1f, 0.8f, 0.1f, 1f);      // Green (Tama)
    public Color wrongColor = new Color(0.8f, 0.1f, 0.1f, 1f);        // Red (Mali)
    public Color panelBackgroundColor = new Color(0.02f, 0.05f, 0.2f, 0.9f);

    void Start()
    {
        ApplyBaseStyle();
    }

    void ApplyBaseStyle()
    {
        if (darkBackgroundPanel != null)
        {
            darkBackgroundPanel.color = panelBackgroundColor;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            SetButtonImageColor(answerButtons[index], normalButtonColor);

            answerButtons[index].onClick.RemoveAllListeners();
            answerButtons[index].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    public void OnAnswerSelected(int buttonIndex)
    {
        SetAllButtonsInteractable(false);

        Button selectedBtn = answerButtons[buttonIndex];

        // 1. Gawing Gold/Orange muna
        SetButtonImageColor(selectedBtn, selectedColor);

        // 2. Suspense delay
        StartCoroutine(RevealAnswerRoutine(selectedBtn, true, buttonIndex));
    }

    IEnumerator RevealAnswerRoutine(Button btn, bool isCorrect, int index)
    {
        yield return new WaitForSeconds(2f);

        // 3. I-update ang kulay patungong Green o Red
        if (isCorrect)
        {
            SetButtonImageColor(btn, correctColor);
            Debug.Log("Tama ang sagot!");
        }
        else
        {
            SetButtonImageColor(btn, wrongColor);
            Debug.Log("Mali ang sagot!");
        }
    }

    // Direktang binabago ang Image component color ng button
    void SetButtonImageColor(Button btn, Color newColor)
    {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = newColor;
        }
    }

    void SetAllButtonsInteractable(bool state)
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = state;
        }
    }
}
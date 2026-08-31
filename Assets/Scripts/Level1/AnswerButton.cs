
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerButton : MonoBehaviour
{
    [SerializeField] private TMP_Text answerText;

    private string answer;
    private Level1Manager levelManager;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(
        string newAnswer,
        Level1Manager manager)
    {
        answer = newAnswer;
        levelManager = manager;

        if (answerText != null)
        {
            answerText.text = answer;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SelectAnswer);

        button.interactable = true;
    }

    private void SelectAnswer()
    {
        Debug.Log("Clicked answer: " + answer);

        if (levelManager != null)
        {
            levelManager.CheckAnswer(answer);
        }
        else
        {
            Debug.LogError(
                "Level1Manager is not assigned!"
            );
        }
    }
}


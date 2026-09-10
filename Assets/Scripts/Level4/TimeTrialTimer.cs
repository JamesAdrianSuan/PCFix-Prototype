using UnityEngine;
using TMPro;

public class TimeTrialTimer : MonoBehaviour
{
    [Header("Timer")]
    public float timeLimit = 60f;

    [Header("UI")]
    public TMP_Text timerText;

    private float currentTime;
    private bool timerRunning = false;

    private void Start()
    {
        currentTime = timeLimit;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            timerRunning = false;

            UpdateTimerDisplay();

            Debug.Log("TIME TRIAL FAILED!");
            return;
        }

        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        currentTime = timeLimit;
        timerRunning = true;

        Debug.Log("TIME TRIAL STARTED!");
    }

    public void StopTimer()
    {
        timerRunning = false;

        Debug.Log(
            "TIME TRIAL STOPPED! Time: " +
            currentTime.ToString("F1")
        );
    }

    public void ResetTimer()
    {
        currentTime = timeLimit;
        timerRunning = false;

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(
            currentTime / 60f
        );

        int seconds = Mathf.FloorToInt(
            currentTime % 60f
        );

        timerText.text =
            string.Format(
                "{0:00}:{1:00}",
                minutes,
                seconds
            );
    }
}
using UnityEngine;
using TMPro;

public class TimeTrialTimer : MonoBehaviour
{
    [Header("Timer")]
    public bool timerRunning = false;

    [Header("UI")]
    public TMP_Text timerText;

    private float elapsedTime = 0f;

    public float ElapsedTime => elapsedTime;
    public bool TimerRunning => timerRunning;

    private void Start()
    {
        elapsedTime = 0f;
        timerRunning = false;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;

        Debug.Log("TIME TRIAL STARTED!");
    }

    public void StopTimer()
    {
        timerRunning = false;

        Debug.Log(
            "TIME TRIAL COMPLETED! Time: " +
            FormatTime()
        );
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerRunning = false;

        UpdateTimerDisplay();
    }

    public string GetFormattedTime()
    {
        return FormatTime();
    }

    private string FormatTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        timerText.text = FormatTime();
    }
}
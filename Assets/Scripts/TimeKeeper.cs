using System.Collections;
using TMPro;
using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public UIManager settingsManager;

    private int timerSeconds;
    private int currentSeconds = 0;
    private bool isPaused = false;
    private IEnumerator timerCoroutine;

    private Pomodoro currentPomodoro;
    
    private void Start()
    {
        DisplayTime(timerSeconds);
    }

    public void ToggleTimer()
    {
        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            ResetTimeDisplay();
        }
        else
        {
            timerCoroutine = StartCountdown(timerSeconds);
            // reset pause
            ResumeTimer();
            StartCoroutine(timerCoroutine);
        }
    }

    public void LoadCurrentPomodoro()
    {
        //timerSeconds = ProfileData.Current.Pomodoros[0].Duration;
        currentPomodoro = ProfileData.Current.Pomodoros[ProfileData.Current.PomodoroPlayIndex];
        timerSeconds = currentPomodoro.Duration;
        settingsManager.DisplayCurrentPomodoroName(currentPomodoro.Name);
    }

    public void PauseTimer()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void MenuPause()
    {
        Time.timeScale = 0f;
    }

    public void ResumeTimer()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void MenuResume()
    {
        if(!isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    private IEnumerator StartCountdown(int secondsCount)
    {
        currentSeconds = secondsCount;

        while(currentSeconds >= 0)
        {
            DisplayTime(currentSeconds);
            yield return new WaitForSeconds(1f);
            currentSeconds--;
        }

        // this is where the timer ends
        ResetTimeDisplay();

        // wait for another second before starting the next timer
        // this way the 00 timer is visible
        // yield return new WaitForSeconds(1f);
        // play the end sound after the 00 time has been shown for one second
        settingsManager.PlayEndSound(currentPomodoro.SoundPath);

        //Debug.Log(currentPomodoro.Name);

        currentPomodoro = ProfileData.Current.GetNextPomodoroToPlay();
        timerSeconds = currentPomodoro.Duration;
        settingsManager.DisplayCurrentPomodoroName(currentPomodoro.Name);
        timerCoroutine = StartCountdown(timerSeconds);
        // reset pause
        // ResumeTimer();
        StartCoroutine(timerCoroutine);
    }

    private void ResetTimeDisplay()
    {
        DisplayTime(0);
        timerCoroutine = null;
    }

    private void DisplayTime(int time)
    {
        timerText.text = TimeFormatter(time);
    }

    public void ResetCurrentTime()
    {
        StopCoroutine(timerCoroutine);
        timerCoroutine = null;
        timerText.text = TimeFormatter(timerSeconds);
    }

    private string TimeFormatter(int time)
    {
        int minutes = time / 60;
        int seconds = time % 60;

        return TimeIntervalFormatter(minutes) + ":" + TimeIntervalFormatter(seconds);
    }

    private string TimeIntervalFormatter(int timeInterval)
    {
        if(timeInterval < 10)
        {
            return "0" + timeInterval.ToString();
        }
        else
        {
            return timeInterval.ToString();
        }
    }
}

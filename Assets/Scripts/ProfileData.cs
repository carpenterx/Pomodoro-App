using System;
using System.Collections.Generic;

[Serializable]
public class ProfileData
{
    public string ButtonNormalColor;
    public string ButtonHighlightedColor;
    public int PomodoroPlayIndex = 0;
    public int SelectedPomodoroIndex = -1;
    public List<Pomodoro> Pomodoros;
    public string BackgroundImagePath;
    public string Volume = "1";

    public ProfileData()
    {
        Pomodoros = new List<Pomodoro>();
    }

    public void SetSelectedPomodoro(int index)
    {
        SelectedPomodoroIndex = index;
    }

    public void ResetSelectedPomodoro()
    {
        SelectedPomodoroIndex = -1;
    }

    public Pomodoro GetNextPomodoroToPlay()
    {
        if(PomodoroPlayIndex == Pomodoros.Count - 1)
        {
            PomodoroPlayIndex = 0;
            return Pomodoros[0];
        }
        else
        {
            PomodoroPlayIndex++;
            return Pomodoros[PomodoroPlayIndex];
        }
    }
}

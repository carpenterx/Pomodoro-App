using System;

[Serializable]
public class Pomodoro
{
    public string Name;
    public int Duration;
    public string SoundPath;

    public Pomodoro(string name, int duration, string soundPath)
    {
        Name = name;
        Duration = duration;
        SoundPath = soundPath;
    }

    public Pomodoro(string name, string durationStr, string soundPath)
    {
        Name = name;
        int duration;
        if(int.TryParse(durationStr, out duration))
        {
            Duration = duration;
        }
        else
        {
            Duration = 0;
        }
        SoundPath = soundPath;
    }
}

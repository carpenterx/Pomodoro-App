using System;

[Serializable]
public class Pomodoro
{
    public string Name;
    public int Duration;
    public string SoundName;

    public Pomodoro(string name, int duration, string soundName)
    {
        Name = name;
        Duration = duration;
        SoundName = soundName;
    }

    public Pomodoro(string name, string durationStr, string soundName)
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
        SoundName = soundName;
    }
}

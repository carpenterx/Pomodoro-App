using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProfileData
{
    private static ProfileData _current;
    public static ProfileData Current
    {
        get
        {
            if (_current == null)
            {
                _current = new ProfileData();
            }
            return _current;
        }
        set
        {
            _current = value;
        }
    }

    public static string FolderName = Application.persistentDataPath + "/profiles";
    public static string FileName = "";
    public static string DefaultFileName = "[autosave]";
    public static string FileExtension = ".pro";
    public static string SavePath = "";

    public string ButtonNormalColor;
    public string ButtonHighlightedColor;
    public int PomodoroPlayIndex = 0;
    public static int SelectedPomodoroIndex = -1;
    public List<Pomodoro> Pomodoros;
    public string BackgroundImagePath;
    public string Volume = "1";

    public ProfileData()
    {
        FileName = DefaultFileName;
        Pomodoros = new List<Pomodoro>();
        SavePath = BuildSavePath();
    }

    public static void ChangeFileName(string fileName)
    {
        FileName = fileName;
        SavePath = BuildSavePath();
    }

    private static string BuildSavePath()
    {
        return FolderName + "/" + FileName + FileExtension;
    }

    public static void SetSelectedPomodoro(int index)
    {
        SelectedPomodoroIndex = index;
    }

    public static void ResetSelectedPomodoro()
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

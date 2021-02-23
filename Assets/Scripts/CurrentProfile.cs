using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu]
public class CurrentProfile : ScriptableObject
{
    public string FolderName;
    public string FileName = "";
    public string DefaultFileName = "[autosave]";
    public string FileExtension = ".pro";
    public string SavePath = "";
    public ProfileData profileData = new ProfileData();

    private void OnEnable()
    {
        FolderName = Application.persistentDataPath + "/profiles";
        FileName = "";
        DefaultFileName = "[autosave]";
        FileExtension = ".pro";
        SavePath = "";
}

    public void ChangeFileName(string fileName)
    {
        FileName = fileName;
        SavePath = BuildSavePath();
    }

    private string BuildSavePath()
    {
        return FolderName + "/" + FileName + FileExtension;
    }

    public ProfileData LoadProfile(string profileName)
    {
        ChangeFileName(profileName);
        return (ProfileData)JsonIO.Load(SavePath);
    }

    public ProfileData LoadDefaultProfile()
    {
        return LoadProfile(DefaultFileName);
    }

    public void SaveProfile(string profileName)
    {
        if (!Directory.Exists(FolderName))
        {
            Directory.CreateDirectory(FolderName);
        }
        ChangeFileName(profileName);
        JsonIO.Save(SavePath, profileData);
    }
}
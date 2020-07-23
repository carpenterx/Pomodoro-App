using System.IO;
using System.Text;
using UnityEngine;

public static class JsonSave
{
    public static void Save(ProfileData saveData)
    {
        string savePath = ProfileData.SavePath;
        string saveDir = ProfileData.FolderName;
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        string jsonString = JsonUtility.ToJson(saveData, true);

        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

            stream.Write(bytes, 0, bytes.Length);
        }
    }

    public static object Load(ProfileData saveData)
    {
        string savePath = ProfileData.SavePath;
        if (File.Exists(savePath))
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                string jsonString = "";
                byte[] buffer = new byte[1024];
                int c;
                while ((c = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    jsonString += Encoding.UTF8.GetString(buffer, 0, c);
                }
                return JsonUtility.FromJson<ProfileData>(jsonString);
            }
        }
        return null;
    }
}
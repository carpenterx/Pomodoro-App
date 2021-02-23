using System.IO;
using System.Text;
using UnityEngine;

public static class JsonIO
{
    public static void Save(string filePath, ProfileData saveData)
    {
        //string savePath = ProfileData.SavePath;
        //string saveDir = ProfileData.FolderName;
        
        string jsonString = JsonUtility.ToJson(saveData, true);

        using (FileStream stream = new FileStream(path: filePath, FileMode.Create))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

            stream.Write(bytes, 0, bytes.Length);
        }
    }

    public static object Load(string filePath)
    {
        //ProfileData.ChangeFileName(profileName);
        //string savePath = ProfileData.SavePath;
        if (File.Exists(filePath))
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
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
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class BackgroundImage : MonoBehaviour
{
    public Image image;

    public void BrowseToBackGround()
    {
        var extenstions = new[] { new ExtensionFilter("Image Files", "jpg", "png") };
        string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel("Open Sound File", "", extenstions, false);
        if(selectedFiles.Length > 0)
        {
            ChangeBackgroundImage(selectedFiles[0]);
        }
    }

    public void ChangeBackgroundImage(string imagePath)
    {
        if(File.Exists(imagePath))
        {
            byte[] imageData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            texture.LoadImage(imageData);
            texture.name = Path.GetFileNameWithoutExtension(imagePath);
            Sprite mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            image.sprite = mySprite;
            image.type = Image.Type.Tiled;
            //image.type = Image.Type.Simple;
            ProfileData.Current.BackgroundImagePath = imagePath;
        }
    }
}

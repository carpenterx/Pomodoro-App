using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class BackgroundImage : MonoBehaviour
{
    public Image image;
    public Image previewImage;

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
            image.color = Color.white;
            image.sprite = mySprite;
            //image.type = Image.Type.Tiled;
            previewImage.color = Color.white;
            previewImage.sprite = mySprite;
            previewImage.preserveAspect = true;
            //image.type = Image.Type.Simple;
            ProfileData.Current.BackgroundImagePath = imagePath;
        }
        else
        {
            RemoveBackgroundImage();
        }
    }

    public void RemoveBackgroundImage()
    {
        image.sprite = null;
        image.color = new Color(13f / 255f, 17f / 255f, 23f / 255f);
        previewImage.sprite = null;
        previewImage.color = new Color(13f / 255f, 17f / 255f, 23f / 255f);
        ProfileData.Current.BackgroundImagePath = "";
    }
}
